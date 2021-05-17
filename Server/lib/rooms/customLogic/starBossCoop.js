"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const starBossWorm_1 = require("./starBossWorm");
const logger = require("../../helpers/logger.js");
const utilities = require('../../helpers/LSUtilities.js');
// string indentifiers for keys in the room attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const ClientReadyState = "readyState";
const GeneralMessage = "generalMessage";
const BeginRoundCountDown = "countDown";
const BossHealth = "bossHealth";
const BossReadyState = "bossReadyState";
/** Enum for game state */
const StarBossServerGameState = {
    None: "None",
    Waiting: "Waiting",
    BeginRound: "BeginRound",
    SimulateRound: "SimulateRound",
    EndRound: "EndRound"
};
/** Enum for begin round count down */
const StarBossCountDownState = {
    Enter: "Enter",
    GetReady: "GetReady",
    CountDown: "CountDown"
};
/** Enum for the boss state */
const StarBossBossState = {
    Waiting: "Waiting",
    SendPath: "SendPath",
    Defeated: "Defeated"
};
/** Max health for the boss */
const BossStartHealth = 100;
/** Count down time before a round begins */
const StarBossCountDownTime = 3;
/** Maximum random variation of the boss path in the X & Z axes */
const MaxPathVariation = 100;
let starBossCoopCustomMethods = {};
/**
 * The primary game loop on the server
 * @param roomRef Reference to the room
 * @param deltaTime The server delta time in seconds
 */
let gameLoop = function (roomRef, deltaTime) {
    // Update the game state
    switch (getGameState(roomRef, CurrentState)) {
        case StarBossServerGameState.None:
            break;
        case StarBossServerGameState.Waiting:
            waitingLogic(roomRef, deltaTime);
            break;
        case StarBossServerGameState.BeginRound:
            beginRoundLogic(roomRef, deltaTime);
            break;
        case StarBossServerGameState.SimulateRound:
            simulateRoundLogic(roomRef, deltaTime);
            break;
        case StarBossServerGameState.EndRound:
            endRoundLogic(roomRef, deltaTime);
            break;
        default:
            logger.error("Unknown Game State - " + getGameState(roomRef, CurrentState));
            break;
    }
};
// Client Request Logic
// These functions get called by the client in the form of the "customMethod" message set up in the room.
//======================================
/**
 * Track the boss taking damage
 * @param {*} roomRef Reference to the room
 * @param {*} client The Client reporting the damage
 * @param {*} request Data including which entity and how much damage
 */
starBossCoopCustomMethods.bossTookDamage = function (roomRef, client, request) {
    if (getGameState(roomRef, CurrentState) != StarBossServerGameState.SimulateRound) {
        logger.silly("Cannot score a hit until the game has begun!");
        return;
    }
    const param = request.param;
    // 0 = entity Id | 1 = damage dealt
    if (param == null || param.length < 2) {
        throw "Error - Boss Took Damage - Missing parameter";
        return;
    }
    const entityId = param[0];
    const damage = Number(param[1]);
    if (isNaN(damage)) {
        throw `Invalid damage - ${damage}`;
        return;
    }
    if (roomRef.boss) {
        roomRef.boss.health -= damage;
        // Get the networked entity
        let entity = roomRef.state.networkedEntities.get(entityId);
        if (entity == null) {
            logger.error(`*** No entity with that Id: ${entityId} ***`);
            return;
        }
        let currentScore = entity.attributes.get("score");
        let currentScoreNum = 0;
        let scoreMessage;
        if (currentScore) {
            // Parse current score to number
            let currentScoreNum = Number(currentScore);
            if (isNaN(currentScoreNum)) {
                logger.error(`*** Error parsing entity's current score: ${currentScoreNum} ***`);
                currentScoreNum = 0;
            }
            // Increment score by damage dealt
            currentScoreNum += damage;
            scoreMessage = {
                entityId: entityId,
                attributesToSet: { score: currentScoreNum.toString() }
            };
            // Update the player's score
            roomRef.setAttribute(client, scoreMessage);
        }
        else {
            currentScoreNum = damage;
            scoreMessage = {
                entityId: entityId,
                attributesToSet: { score: currentScoreNum.toString() }
            };
            // Update the player's score
            roomRef.setAttribute(client, scoreMessage);
        }
    }
};
//====================================== END Client Request Logic
// GAME LOGIC
//======================================
/**
 * Returns a randomly selected player entity
 * @param roomRef Reference to the room
 */
let getRandomPlayerForBossTarget = function (roomRef) {
    let entities = Array.from(roomRef.state.networkedEntities.values());
    let randomEntity;
    if (entities) {
        if (entities.length > 0) {
            let randomIdx = utilities.getRandomIntInclusive(0, entities.length - 1);
            randomEntity = entities[randomIdx];
        }
    }
    else {
        logger.error(`Error Getting Random Player - Failed to convert Networked Entities into array`);
    }
    if (randomEntity == null) {
        logger.error(`Error Getting Random Player`);
    }
    return randomEntity;
};
/**
 * Returns the game state of the server
 * @param {*} roomRef Reference to the room
 * @param {*} gameState Key for which game state you want, either the Current game state for the Last game state
 */
let getGameState = function (roomRef, gameState) {
    return roomRef.state.attributes.get(gameState);
};
/**
 * Checks if all the connected clients have a 'readyState' of "ready"
 * @param {*} users The collection of users from the room's state
 */
let checkIfUsersReady = function (users) {
    let playersReady = true;
    let userArr = Array.from(users.values());
    if (userArr.length <= 0)
        playersReady = false;
    for (let user of userArr) {
        let readyState = user.attributes.get(ClientReadyState);
        if (readyState == null || readyState != "ready") {
            playersReady = false;
            break;
        }
    }
    return playersReady;
};
/**
 * Checks if all the connected clients' boss is ready for a new path
 * @param {*} users The collection of users from the room's state
 */
let checkIfBossesReady = function (users) {
    let bossesReady = true;
    let userArr = Array.from(users.values());
    if (userArr.length <= 0)
        bossesReady = false;
    for (let user of userArr) {
        let readyState = user.attributes.get(BossReadyState);
        if (readyState == null || readyState != "bossReady") {
            bossesReady = false;
            break;
        }
    }
    return bossesReady;
};
/**
 * Resets data tracking collection and unlocks the room
 * @param roomRef Reference to the room
 */
let resetForNewRound = function (roomRef) {
    setUsersAttribute(roomRef, ClientReadyState, "waiting");
    // Create new instance of the boss with health
    roomRef.boss = new starBossWorm_1.StarBossWorm(BossStartHealth);
    roomRef.CurrentBossState = StarBossBossState.Waiting;
    unlockIfAble(roomRef);
};
let resetPlayerScores = function (roomRef) {
    // Reset all player scores
    setEntitiesAttribute(roomRef, "score", "0");
};
/**
 * Sets attribute of all connected users.
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
let setUsersAttribute = function (roomRef, key, value) {
    for (let entry of Array.from(roomRef.state.networkedUsers)) {
        let userKey = entry[0];
        let msg = { userId: userKey, attributesToSet: {} };
        msg.attributesToSet[key] = value;
        roomRef.setAttribute(null, msg);
    }
};
/**
* Sets attribute of all connected entities.
* @param {*} roomRef Reference to the room
* @param {*} key The key for the attribute you want to set
* @param {*} value The value of the attribute you want to set
*/
let setEntitiesAttribute = function (roomRef, key, value) {
    for (let entry of Array.from(roomRef.state.networkedEntities)) {
        let entityKey = entry[0];
        let msg = { entityId: entityKey, attributesToSet: {} };
        msg.attributesToSet[key] = value;
        roomRef.setAttribute(null, msg);
    }
};
/**
* Sets attriubte of the room
* @param {*} roomRef Reference to the room
* @param {*} key The key for the attribute you want to set
* @param {*} value The value of the attribute you want to set
*/
let setRoomAttribute = function (roomRef, key, value) {
    roomRef.state.attributes.set(key, value);
};
/**
 * Generates and returns new path data for the boss
 * @param roomRef Reference to the room
 */
let generatePathData = function (roomRef) {
    let player = getRandomPlayerForBossTarget(roomRef);
    if (player) {
        let playerPosition = { x: Number(player.xPos), y: Number(player.yPos), z: Number(player.zPos) };
        let playerVelocity = { x: Number(player.xVel), y: Number(player.yVel), z: Number(player.zVel) };
        playerPosition.x += playerVelocity.x * 3;
        playerPosition.y += Math.max(10, playerVelocity.y * 3);
        playerPosition.z += playerVelocity.z * 3;
        let randomX = utilities.getRandomFloatInclusive(1, MaxPathVariation);
        let randomZ = utilities.getRandomFloatInclusive(1, MaxPathVariation);
        let pathData = {
            start: { x: playerPosition.x + randomX, y: playerPosition.y, z: playerPosition.z + randomZ },
            peak: { x: playerPosition.x, y: playerPosition.y + 10, z: playerPosition.z },
            end: { x: playerPosition.x - randomX, y: playerPosition.y, z: playerPosition.z - randomZ }
        };
        return pathData;
    }
    else {
        logger.error(`*** Error generating path data - No target player ***`);
    }
    return null;
};
/**
 * Unlocks the room if space is available
 * @param roomRef Reference to the room
 */
let unlockIfAble = function (roomRef) {
    if (roomRef.hasReachedMaxClients() === false) {
        roomRef.unlock();
    }
};
//====================================== END GAME LOGIC
// GAME STATE LOGIC
//======================================
/**
 * Move the server game state to the new state
 * @param roomRef Reference to the room
 * @param {*} newState The new state to move to
 */
let moveToState = function (roomRef, newState) {
    // LastState = CurrentState
    setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));
    // CurrentState = newState
    setRoomAttribute(roomRef, CurrentState, newState);
};
/**
 * The logic run when the server is in the Waiting state
 * @param roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef, deltaTime) {
    let playersReady = false;
    // Switch on LastState since the waiting logic gets used in multiple places
    switch (getGameState(roomRef, LastState)) {
        case StarBossServerGameState.None:
        case StarBossServerGameState.EndRound:
            // Check if all the users are ready to receive targets
            playersReady = checkIfUsersReady(roomRef.state.networkedUsers);
            // Return out if not all of the players are ready yet.
            if (playersReady == false) {
                setRoomAttribute(roomRef, GeneralMessage, "Waiting for players to ready up...");
                return;
            }
            setRoomAttribute(roomRef, GeneralMessage, "");
            // Lock the room
            roomRef.lock();
            resetPlayerScores(roomRef);
            // Begin a new round
            moveToState(roomRef, StarBossServerGameState.BeginRound);
            break;
    }
};
/**
 * The logic run when the server is in the BeginRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let beginRoundLogic = function (roomRef, deltaTime) {
    switch (roomRef.CurrentCountDownState) {
        // Beginning a new round
        case StarBossCountDownState.Enter:
            // Reset the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "");
            // Broadcast to the clients that a round has begun
            roomRef.broadcast("beginRoundCountDown", {});
            // Reset count down helper value
            roomRef.currCountDown = 0;
            // Move to the GetReady state of the count down
            roomRef.CurrentCountDownState = StarBossCountDownState.GetReady;
            break;
        case StarBossCountDownState.GetReady:
            // Begin with "Get Ready!"
            // Set the count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, "Something lurks beneath the planet's surface... Get ready!");
            // Show the "Get Ready!" message for 3 seconds
            if (roomRef.currCountDown < 3) {
                roomRef.currCountDown += deltaTime;
                return;
            }
            // Move to the CountDown state of the count down
            roomRef.CurrentCountDownState = StarBossCountDownState.CountDown;
            // Set count down helper to the Count Down Time
            roomRef.currCountDown = StarBossCountDownTime;
            break;
        case StarBossCountDownState.CountDown:
            // Update count down message attribute
            setRoomAttribute(roomRef, BeginRoundCountDown, `Incoming in ${Math.ceil(roomRef.currCountDown).toString()}s`);
            // Update Count Down value
            if (roomRef.currCountDown >= 0) {
                roomRef.currCountDown -= deltaTime;
                return;
            }
            // Tell all clients that round has begun!
            roomRef.broadcast("beginRound", { bossHealth: BossStartHealth });
            // Move to the Simulation state
            moveToState(roomRef, StarBossServerGameState.SimulateRound);
            // Clear user's ready state for round begin
            setUsersAttribute(roomRef, ClientReadyState, "waiting");
            // Reset Current Count Down state for next round
            roomRef.CurrentCountDownState = StarBossCountDownState.Enter;
            break;
    }
};
/**
 * The logic run when the server is in the SimulateRound state
 * @param roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let simulateRoundLogic = function (roomRef, deltaTime) {
    // Loop over the Waiting and SendPath logic until the boss's health reaches zero
    if (roomRef.boss.health <= 0) {
        roomRef.boss.health = 0;
        roomRef.CurrentBossState = StarBossBossState.Defeated;
    }
    // Set the boss's health
    setRoomAttribute(roomRef, BossHealth, roomRef.boss.health.toString());
    switch (roomRef.CurrentBossState) {
        case StarBossBossState.Waiting:
            // Check if all the bosses are ready to receive a new path
            let bossesReady = checkIfBossesReady(roomRef.state.networkedUsers);
            // Return out if not all bosses are ready yet.
            if (bossesReady == false) {
                return;
            }
            // Send new path data to all clients
            roomRef.CurrentBossState = StarBossBossState.SendPath;
            break;
        case StarBossBossState.SendPath:
            // generate path data
            let pathData = generatePathData(roomRef);
            // Set all clients boss ready state to be in progress so we'll keep looping over 
            // the waiting logic until all clients report they are ready for new boss path data
            setUsersAttribute(roomRef, BossReadyState, "inProgress");
            // Send message to clients that the boss path data is ready along with the new target path data
            roomRef.broadcast("bossPathReady", pathData);
            roomRef.CurrentBossState = StarBossBossState.Waiting;
            break;
        case StarBossBossState.Defeated:
            // Set the final boss health
            setRoomAttribute(roomRef, BossHealth, roomRef.boss.health.toString());
            // Move to the EndRound state
            moveToState(roomRef, StarBossServerGameState.EndRound);
            break;
    }
};
/**
 * The logic run when the server is in the EndRound state
 * @param roomRef Reference to the room
 * @param {*} deltaTime Server delta time in seconds
 */
let endRoundLogic = function (roomRef, deltaTime) {
    // Let all clients know that the round has ended
    roomRef.broadcast("onRoundEnd", {});
    // Reset the server state for a new round
    resetForNewRound(roomRef);
    // Move to Waiting state, waiting for all players to "ready up" for another round of play
    moveToState(roomRef, StarBossServerGameState.Waiting);
};
//====================================== END GAME STATE LOGIC
// Room accessed functions
//======================================
/**
 * Initialize the Star Boss Co-op logic
 * @param {*} roomRef Reference to the room
 * @param {*} options Options of the room from the client when it was created
 */
exports.InitializeLogic = function (roomRef) {
    roomRef.CurrentBossState = StarBossBossState.Waiting;
    /** The current state of the count down logic */
    roomRef.CurrentCountDownState = StarBossCountDownState.Enter;
    /** Used to help run the count down at the beginning of a new round. */
    roomRef.currCountDown = 0;
    roomRef.currentTime = 0;
    // Set initial game state to waiting for all clients to be ready
    setRoomAttribute(roomRef, CurrentState, StarBossServerGameState.Waiting);
    setRoomAttribute(roomRef, LastState, StarBossServerGameState.None);
    resetForNewRound(roomRef);
};
/**
 * Run Game Loop Logic
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in milliseconds
 */
exports.ProcessLogic = function (roomRef, deltaTime) {
    gameLoop(roomRef, deltaTime / 1000); // convert milliseconds to seconds
};
/**
 * Processes requests from a client to run custom methods
 * @param {*} roomRef Reference to the room
 * @param {*} client Reference to the client the request came from
 * @param {*} request Request object holding any data from the client
 */
exports.ProcessMethod = function (roomRef, client, request) {
    // Check for and run the method if it exists
    if (request.method in starBossCoopCustomMethods && typeof starBossCoopCustomMethods[request.method] === "function") {
        starBossCoopCustomMethods[request.method](roomRef, client, request);
    }
    else {
        throw "No Method: " + request.method + " found";
        return;
    }
};
//====================================== END VME Room accessed functions
