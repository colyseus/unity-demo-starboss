import { Client } from "colyseus";
import { ColyseusRoomState, ColyseusNetworkedEntity, ColyseusNetworkedUser } from "../schema/ColyseusRoomState";
import { StarBossRoom } from "../StarBossRoom";

const logger = require("../../helpers/logger.js");
const utilities = require('../../helpers/LSUtilities.js');

// string indentifiers for keys in the room attributes
const CurrentState = "currentGameState";
const LastState = "lastGameState";
const ClientReadyState = "readyState";
const GeneralMessage = "generalMessage";
const BeginRoundCountDown = "countDown";
const WinningTeamId = "winningTeamId";

const KillScoreMultiplier = 10; //How many points 1 kill is worth

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

/** Count down time before a round begins */
const StarBossCountDownTime: number = 3;

/**
 * The primary game loop on the server
 * @param roomRef Reference to the room
 * @param deltaTime The server delta time in seconds
 */
let gameLoop = function (roomRef: StarBossRoom, deltaTime: number){
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
}

// Client Request Logic
// These functions get called by the client in the form of the "customMethod" message set up in the room.
//======================================
const customMethods: any = {};
/**
 * Called by the client when a player is killed (self-reported)
 * @param roomRef Reference to the room
 * @param client The reporting client
 * @param request In order, the Attacker's ID and the Victim's ID, for scoring purposes
 */
customMethods.playerKilled =  function (roomRef: StarBossRoom, client: Client, request: any) {
    
    //Don't count kills until a round is going
    if(getGameState(roomRef, CurrentState) != StarBossServerGameState.SimulateRound) {
        logger.silly("Cannot score a hit until the game has begun!");
        return;
    }

    const param = request.param;

    // 0 = Attacker ID | 1 = Killed ID
    if(param == null || param.length < 2){
        throw "Missing attacker or killed parameters";
        return;
    }

    const attackerID = param[0];
    const killedID = param[1];
    let attacker = roomRef.state.networkedEntities.get(attackerID);
    let killed = roomRef.state.networkedEntities.get(killedID);
    
    if(attacker != null){
        //Update the killing player's stats
        let killNum: number = getAttributeNumber(attacker, "kills", 0);
        killNum += 1;
        let score: number = killNum * KillScoreMultiplier;
        roomRef.setAttribute(null, { entityId: attackerID, attributesToSet: { kills: killNum.toString(), score: score.toString() } });

        // Update the team's score
        updateTeamScore(roomRef, attackerID, 1);
    }
    else{
        logger.silly(`No attacking entity found with Id: ${attackerID}`)
    }

    if(killed != null){
        //Update death count of the dead player
        let deathNum: number = getAttributeNumber(killed, "deaths", 0);
        deathNum += 1;
        roomRef.setAttribute(null, {entityId: killedID, attributesToSet: {deaths: deathNum.toString() } });
    }
    else{
        logger.silly(`No dead entity found with Id: ${killedID}`)
    }
}
//====================================== END Client Request Logic

// GAME LOGIC
//======================================
/**
 * Retrieve an attribute number from an entity by name
 * @param entity The entity who has the attribute we want
 * @param attributeName The string name of the attribute
 * @param defaultValue If the attribute is not found or is not a number, we return this
 */
let getAttributeNumber = function(entity: ColyseusNetworkedEntity, attributeName: string, defaultValue: number): number {
    let attribute = entity.attributes.get(attributeName);
    let attributeNumber: number = defaultValue;
    if(attribute){
        attributeNumber = Number(attribute);
        if(isNaN(attributeNumber)){
            logger.error(`*** Error parsing entity's attributeNumber: ${attributeNumber} ***`);
            attributeNumber = defaultValue;
        }
    }
    else {
        return defaultValue;
    }

    return attributeNumber;
}

/**
 * Get the score of a given team
 * @param roomRef Reference to the room
 * @param teamIndex The index of the team who's score we want
 */
let getTeamScore = function(roomRef: StarBossRoom, teamIndex: number): number {
    let score: number = Number(roomRef.state.attributes.get(`team_${teamIndex.toString()}_score`));

    if(isNaN(score)) {
        return 0;
    }

    return score;
}

/**
 * Returns the game state of the server
 * @param {*} gameState Key for which game state you want, either the Current game state for the Last game state
 */
let getGameState = function (roomRef: StarBossRoom, gameState: string) {
    return roomRef.state.attributes.get(gameState);
}
 
/**
 * Checks if all the connected clients have a 'readyState' of "ready"
 * @param {*} users The collection of users from the room's state
 */
let checkIfUsersReady = function(users: Map<string, ColyseusNetworkedUser>) {
    let playersReady = true;

    let userArr: ColyseusNetworkedUser[] = Array.from<ColyseusNetworkedUser>(users.values());

    if(userArr.length <= 0)
        playersReady = false;

    for(let user of userArr) {
        
        let readyState = user.attributes.get(ClientReadyState);
        
        if(readyState == null || readyState != "ready"){
            playersReady = false;
            break;
        }
    }

    return playersReady;
}

/** Resets data tracking collection and unlocks the room */
let resetForNewRound = function (roomRef: StarBossRoom) {
    
    setUsersAttribute(roomRef, ClientReadyState, "waiting");

    unlockIfAble(roomRef);
}

let resetPlayerScores = function(roomRef: StarBossRoom) {
    // Reset all player scores
    setEntitiesAttribute(roomRef, "score", "0");
    setEntitiesAttribute(roomRef, "kills", "0");
    setEntitiesAttribute(roomRef, "deaths", "0");
    //Remove winning team
    if(roomRef.state.attributes.has(WinningTeamId))
    {
        roomRef.state.attributes.delete(WinningTeamId);
    }
}

/**
 * Reset the score for each team to zero
 * @param roomRef Reference to the room
 */
let resetTeamScores = function(roomRef: StarBossRoom) {

    // Set teams initial score
    roomRef.teams.forEach((teamMap, teamIdx) => {
        setRoomAttribute(roomRef, `team_${teamIdx.toString()}_score`, "0");
    });
    
}

let updateTeamScore = function(roomRef: StarBossRoom, teamMateId: string, amount: number) {

    let teamIdx: number = -1;
    let clientId: string = "";
    let teamScore: number = -1;

    // Get client Id from entity
    let entity: ColyseusNetworkedEntity = roomRef.state.networkedEntities.get(teamMateId);

    if(entity) {
        clientId = entity.ownerId;
        
        // Update the score of the team the clientId belongs to
        roomRef.teams.forEach((teamMap, team) => {
            if(teamIdx == -1 && teamMap.has(clientId)) {
                teamIdx = team;
            }
        });

        if(teamIdx >= 0) {
            teamScore = getTeamScore(roomRef, teamIdx);

            teamScore += amount;
    
            setRoomAttribute(roomRef, `team_${teamIdx}_score`, teamScore.toString());
        }
        else {
            logger.error(`Update Team Score - Error - No team found for client Id: ${clientId}`);
        }
        
    }
    else {
        logger.error(`Update Team Score - Error - No entity found with Id: ${teamMateId}`);
    }

}

/**
 * Sets attribute of all connected users.
 * @param {*} roomRef Reference to the room
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
let setUsersAttribute = function(roomRef: StarBossRoom, key: string, value: string) {
    
    for(let entry of Array.from<any>(roomRef.state.networkedUsers)) {
  
        let userKey = entry[0];
        let userValue = entry[1];
        let msg: any = {userId: userKey, attributesToSet: {}};
  
        msg.attributesToSet[key] = value;
  
        roomRef.setAttribute(null, msg);
    }
    
  }

  /**
 * Sets attribute of all connected entities.
 * @param {*} key The key for the attribute you want to set
 * @param {*} value The value of the attribute you want to set
 */
  let setEntitiesAttribute = function(roomRef: StarBossRoom, key: string, value: string) {
    for(let entry of Array.from<any>(roomRef.state.networkedEntities)) {
  
        let entityKey = entry[0];
        let entityValue = entry[1];
        let msg: any = {entityId: entityKey, attributesToSet: {}};
  
        msg.attributesToSet[key] = value;
  
        roomRef.setAttribute(null, msg);
    }
  }
  
  /**
  * Sets attriubte of the room
  * @param {*} roomRef Reference to the room
  * @param {*} key The key for the attribute you want to set
  * @param {*} value The value of the attribute you want to set
  */
  let setRoomAttribute = function(roomRef: StarBossRoom, key: string, value: string) {
    roomRef.state.attributes.set(key, value);
  }

let unlockIfAble = function (roomRef: StarBossRoom) {
    if(roomRef.hasReachedMaxClients() === false) {
        roomRef.unlock();
    }
}

let checkIfEnoughPlayers = function(roomRef: StarBossRoom): boolean {

    let enough: boolean = true;

    if(roomRef.state.networkedUsers.size < 2) {
        // Number of players to play has dropped too low to continue, end the round
        enough = false;
    }
    
    // Check if either team does not have any players
    roomRef.teams.forEach((teamMap, teamIdx) => {
        
        if(teamMap.size == 0) {
            // This team no longer has any players 
            enough = false;
        }
    });

    return enough;
}
//====================================== END GAME LOGIC

// GAME STATE LOGIC
//======================================
/**
 * Move the server game state to the new state
 * @param {*} newState The new state to move to
 */
let moveToState = function (roomRef: StarBossRoom, newState: string) {

    //logger.silly(`*** Move State From = ${getGameState(CurrentState)} To = ${newState}`);

    // LastState = CurrentState
    setRoomAttribute(roomRef, LastState, getGameState(roomRef, CurrentState));
            
    // CurrentState = newState
    setRoomAttribute(roomRef, CurrentState, newState);
}

/**
 * The logic run when the server is in the Waiting state
 * @param {*} deltaTime Server delta time in seconds
 */
let waitingLogic = function (roomRef: StarBossRoom, deltaTime: Number) {
    
    let playersReady: boolean = false;
    let enoughPlayers: boolean = false;

    // Switch on LastState since the waiting logic gets used in multiple places
    switch(getGameState(roomRef, LastState)){
        case StarBossServerGameState.None:
        case StarBossServerGameState.EndRound:
            
            // Check if all the users are ready to receive targets
            playersReady = checkIfUsersReady(roomRef.state.networkedUsers);
            enoughPlayers = checkIfEnoughPlayers(roomRef);

            // Return out if not all of the players are ready yet.
            if(playersReady == false || enoughPlayers == false) { 
                
                setRoomAttribute(roomRef, GeneralMessage, `${(playersReady == false ? "Waiting for players to ready up." : "")}${(enoughPlayers == false ? " There aren't enough players to begin." : "")}`);

                return;
            }

            setRoomAttribute(roomRef, GeneralMessage, "");

            // Lock the room
            roomRef.lock();

            resetPlayerScores(roomRef);
            resetTeamScores(roomRef);

            // Begin a new round
            moveToState(roomRef, StarBossServerGameState.BeginRound);
            break;
    }
}

/**
 * The logic run when the server is in the BeginRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let beginRoundLogic = function (roomRef: StarBossRoom, deltaTime: number) {
    
    switch(roomRef.CurrentCountDownState) {
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
            setRoomAttribute(roomRef, BeginRoundCountDown, "TEAM DEATHMATCH!");
            
            // Show the "Get Ready!" message for 3 seconds
            if(roomRef.currCountDown < 3){
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
            setRoomAttribute(roomRef, BeginRoundCountDown, `Starting in ${Math.ceil(roomRef.currCountDown).toString()}s`);

            // Update Count Down value
            if(roomRef.currCountDown >= 0){
                roomRef.currCountDown -= deltaTime;
                return;
            }
            
            // TODO: beginRound is expecting a boss health
            let fakeHealth = -1;
            roomRef.broadcast("beginRound", { fakeHealth });

            // Move to the Simulation state
            moveToState(roomRef, StarBossServerGameState.SimulateRound);

            // Clear user's ready state for round begin
            setUsersAttribute(roomRef, ClientReadyState, "waiting");

            // Reset Current Count Down state for next round
            roomRef.CurrentCountDownState = StarBossCountDownState.Enter;
            break;
    }

    
}

/**
 * The logic run when the server is in the SimulateRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let simulateRoundLogic = function (roomRef: StarBossRoom, deltaTime: Number) {
    
    // Check if there are enough players to continue
    if(checkIfEnoughPlayers(roomRef) == false) {

        // End round since there are not enough players on a team to finish the round
        moveToState(roomRef, StarBossServerGameState.EndRound);

        return;
    }

    roomRef.teams.forEach((teamMap, teamIdx) => {
        let teamScore: number = getTeamScore(roomRef, teamIdx);

        // Check if the team score is enough to win
        if(getGameState(roomRef, CurrentState) == StarBossServerGameState.SimulateRound && teamScore >= roomRef.tdmScoreToWin) {
            
            setRoomAttribute(roomRef, WinningTeamId, teamIdx.toString());

            moveToState(roomRef, StarBossServerGameState.EndRound);
        }
    });
}

/**
 * The logic run when the server is in the EndRound state
 * @param {*} deltaTime Server delta time in seconds
 */
let endRoundLogic = function (roomRef: StarBossRoom, deltaTime: Number) {

    // Let all clients know that the round has ended
    roomRef.broadcast("onRoundEnd", { });

    // Reset the server state for a new round
    resetForNewRound(roomRef);

    // Move to Waiting state, waiting for all players to "ready up" for another round of play
    moveToState(roomRef, StarBossServerGameState.Waiting);
}

let alertClientsOfTeamChange = function (roomRef: StarBossRoom, clientID: string, teamIndex: Number, added: boolean){
    roomRef.broadcast("onTeamUpdate", { teamIndex: teamIndex, clientID: clientID, added: added.toString()});
}
//====================================== END GAME STATE LOGIC

// VME Room accessed functions
//======================================
/**
 * Initialize the Star Boss Co-op logic
 * @param {*} roomRef Reference to the room
 * @param {*} options Options of the room from the client when it was created
 */
exports.InitializeLogic = function (roomRef: StarBossRoom, options: any) {

    logger.silly(`*** Star Boss TEAM DEATHMATCH Logic Initialize ***`);
    /** The current state of the count down logic */
    roomRef.CurrentCountDownState = StarBossCountDownState.Enter;

    /** Used to help run the count down at the beginning of a new round. */
    roomRef.currCountDown = 0;

    roomRef.currentTime = 0;

    roomRef.tdmScoreToWin = options["scoreToWin"] ? Number(options["scoreToWin"]) : 10;

    logger.silly(`*** TDM - Score to win = ${roomRef.tdmScoreToWin} ***`);

    //If we ever want more than 2 teams, this will need to be updated
    roomRef.teams = new Map();
    roomRef.teams.set(0, new Map());
    roomRef.teams.set(1, new Map());

    // Set initial game state to waiting for all clients to be ready
    setRoomAttribute(roomRef, CurrentState, StarBossServerGameState.Waiting)
    setRoomAttribute(roomRef, LastState, StarBossServerGameState.None);

    resetForNewRound(roomRef);
}

/**
 * Run Game Loop Logic
 * @param {*} roomRef Reference to the room
 * @param {*} deltaTime Server delta time in milliseconds
 */
exports.ProcessLogic = function (roomRef: StarBossRoom, deltaTime: number) {
    
    gameLoop(roomRef, deltaTime / 1000); // convert milliseconds to seconds
}

/**
 * Processes requests from a client to run custom methods
 * @param {*} roomRef Reference to the room
 * @param {*} client Reference to the client the request came from
 * @param {*} request Request object holding any data from the client
 */ 
exports.ProcessMethod = function (roomRef: StarBossRoom, client: Client, request: any) {
    
    // Check for and run the method if it exists
    if (request.method in customMethods && typeof customMethods[request.method] === "function") {
        customMethods[request.method](roomRef, client, request);
    } else {
        throw "No Method: " + request.method + " found";
        return; 
    }
}

/**
 * Process report of a user leaving. If we were previously locked due to a game starting and didn't
 * unlock at the end because the room was full, we'll need to unlock now
 */ 
 exports.ProcessUserLeft = function (roomRef: StarBossRoom, client: Client) {
    if(roomRef.locked)
    {
        switch(getGameState(roomRef, CurrentState)){
        case StarBossServerGameState.Waiting:
            unlockIfAble(roomRef);
            break;
        case StarBossServerGameState.BeginRound:
        case StarBossServerGameState.SimulateRound:
        case StarBossServerGameState.EndRound:
            logger.silly(`Will not unlock the room, Game State - ${getGameState(roomRef, CurrentState)}`);
            break;
        }
    }

    //Remove player from their team
    roomRef.teams.forEach((playerMap, teamIdx) =>{
        if(playerMap.has(client.id))
        {
            playerMap.delete(client.id);
            alertClientsOfTeamChange(roomRef, client.id, teamIdx, false);
        }
    });
 }

 /**
 * Process report of a user leaving. If we were previously locked due to a game starting and didn't
 * unlock at the end because the room was full, we'll need to unlock now
 */ 
 exports.ProcessUserJoined = function (roomRef: StarBossRoom, client: Client) {
    let desiredTeam = -1;
    let currMin = 99999;
    let map = new Map();
    roomRef.teams.forEach((playerMap, teamIdx) =>{
        //Alert the incoming client of the current teams
        client.send("onReceiveTeam", { teamIndex: teamIdx, clients: Array.from(playerMap.keys()) });
        if(playerMap.size < currMin){
            currMin = playerMap.size;
            desiredTeam = teamIdx;
            map = playerMap;
        }
    });

    map.set(client.id, client);
    roomRef.teams.set(desiredTeam, map);
    //Alert the clients of a new player
    alertClientsOfTeamChange(roomRef, client.id, desiredTeam, true);
 }
//====================================== END Room accessed functions