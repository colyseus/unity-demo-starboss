"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.StarBossRoom = void 0;
const colyseus_1 = require("colyseus");
const ColyseusRoomState_1 = require("./schema/ColyseusRoomState");
const logger = require("../helpers/logger");
class StarBossRoom extends colyseus_1.Room {
    constructor() {
        super(...arguments);
        this.clientEntities = new Map();
        this.serverTime = 0;
        this.customMethodController = null;
        this.tdmScoreToWin = 10;
    }
    /**
     * Getter function to retrieve the correct customLogic file. Will try .JS extension and then .TS
     * @param {*} fileName
     */
    getCustomLogic(fileName) {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                this.customMethodController = yield Promise.resolve().then(() => __importStar(require('./customLogic/' + fileName)));
            }
            catch (e) {
                logger.error(e);
            }
            return this.customMethodController;
        });
    }
    /**
     * Callback for the "customMethod" message from the client to run a custom function within the custom logic.
     * Function name is sent from a client.
     * @param {*} client
     * @param {*} request
     */
    onCustomMethod(client, request) {
        try {
            if (this.customMethodController != null) {
                this.customMethodController.ProcessMethod(this, client, request);
            }
            else {
                logger.debug("NO Custom Method Logic Set");
            }
        }
        catch (error) {
            logger.error("Error with custom Method logic: " + error);
        }
    }
    /**
     * Callback for the "entityUpdate" message from the client to update an entity
     * @param {*} clientID
     * @param {*} data
     */
    onEntityUpdate(clientID, data) {
        if (this.state.networkedEntities.has(`${data[0]}`) === false)
            return;
        let stateToUpdate = this.state.networkedEntities.get(data[0]);
        let startIndex = 1;
        if (data[1] === "attributes")
            startIndex = 2;
        for (let i = startIndex; i < data.length; i += 2) {
            const property = data[i];
            let updateValue = data[i + 1];
            if (updateValue === "inc") {
                updateValue = data[i + 2];
                updateValue = parseFloat(stateToUpdate.attributes.get(property)) + parseFloat(updateValue);
                i++; // inc i once more since we had a inc;
            }
            if (startIndex == 2) {
                stateToUpdate.attributes.set(property, updateValue.toString());
            }
            else {
                stateToUpdate[property] = updateValue;
            }
        }
        stateToUpdate.timestamp = parseFloat(this.serverTime.toString());
    }
    /**
     * Callback for when the room is created
     * @param {*} options The room options sent from the client when creating a room
     */
    onCreate(options) {
        return __awaiter(this, void 0, void 0, function* () {
            logger.info("*********************** STAR BOSS ROOM CREATED ***********************");
            console.log(options);
            logger.info("***********************");
            this.maxClients = 32;
            this.roomOptions = options;
            this.teams = new Map();
            if (options["roomId"] != null) {
                this.roomId = options["roomId"];
            }
            // Set the room state
            this.setState(new ColyseusRoomState_1.ColyseusRoomState());
            // Set the callback for the "ping" message for tracking server-client latency
            this.onMessage("ping", (client) => {
                client.send(0, { serverTime: this.serverTime });
            });
            // Set the callback for the "customMethod" message
            this.onMessage("customMethod", (client, request) => {
                this.onCustomMethod(client, request);
            });
            // Set the callback for the "entityUpdate" message
            this.onMessage("entityUpdate", (client, entityUpdateArray) => {
                if (this.state.networkedEntities.has(`${entityUpdateArray[0]}`) === false)
                    return;
                this.onEntityUpdate(client.id, entityUpdateArray);
            });
            // Set the callback for the "removeFunctionCall" message
            this.onMessage("remoteFunctionCall", (client, RFCMessage) => {
                //Confirm Sending Client is Owner 
                if (this.state.networkedEntities.has(`${RFCMessage.entityId}`) === false)
                    return;
                RFCMessage.clientId = client.id;
                // Broadcast the "remoteFunctionCall" to all clients except the one the message originated from
                this.broadcast("onRFC", RFCMessage, RFCMessage.target == 0 ? {} : { except: client });
            });
            // Set the callback for the "setAttribute" message to set an entity or user attribute
            this.onMessage("setAttribute", (client, attributeUpdateMessage) => {
                this.setAttribute(client, attributeUpdateMessage);
            });
            // Set the callback for the "removeEntity" message
            this.onMessage("removeEntity", (client, removeId) => {
                if (this.state.networkedEntities.has(removeId)) {
                    this.state.networkedEntities.delete(removeId);
                }
            });
            // Set the callback for the "createEntity" message
            this.onMessage("createEntity", (client, creationMessage) => {
                // Generate new UID for the entity
                let entityViewID = colyseus_1.generateId();
                let newEntity = new ColyseusRoomState_1.ColyseusNetworkedEntity().assign({
                    id: entityViewID,
                    ownerId: client.id,
                    timestamp: this.serverTime
                });
                let userName = entityViewID;
                if (creationMessage.attributes["userName"] != null) {
                    userName = creationMessage.attributes["userName"];
                }
                if (creationMessage.creationId != null)
                    newEntity.creationId = creationMessage.creationId;
                newEntity.timestamp = parseFloat(this.serverTime.toString());
                for (let key in creationMessage.attributes) {
                    if (key === "creationPos") {
                        newEntity.xPos = parseFloat(creationMessage.attributes[key][0]);
                        newEntity.yPos = parseFloat(creationMessage.attributes[key][1]);
                        newEntity.zPos = parseFloat(creationMessage.attributes[key][2]);
                    }
                    else if (key === "creationRot") {
                        newEntity.xRot = parseFloat(creationMessage.attributes[key][0]);
                        newEntity.yRot = parseFloat(creationMessage.attributes[key][1]);
                        newEntity.zRot = parseFloat(creationMessage.attributes[key][2]);
                        newEntity.wRot = parseFloat(creationMessage.attributes[key][3]);
                    }
                    else {
                        newEntity.attributes.set(key, creationMessage.attributes[key].toString());
                    }
                }
                // Add the entity to the room state's networkedEntities map 
                this.state.networkedEntities.set(entityViewID, newEntity);
                // Add the entity to the client entities collection
                if (this.clientEntities.has(client.id)) {
                    this.clientEntities.get(client.id).push(entityViewID);
                }
                else {
                    this.clientEntities.set(client.id, [entityViewID]);
                }
                logger.silly(`*** Send Player Joined Message  - User Name = ${userName}***`);
                this.broadcast("playerJoined", { userName: userName }, { except: client });
            });
            // Set the frequency of the patch rate
            this.setPatchRate(1000 / 20);
            // Retrieve the custom logic for the room
            const customLogic = yield this.getCustomLogic(options["logic"]);
            if (customLogic == null)
                logger.debug("NO Custom Logic Set");
            try {
                if (customLogic != null) {
                    this.setMetadata({ isCoop: options["logic"] == "starBossCoop" });
                    customLogic.InitializeLogic(this, options);
                }
            }
            catch (error) {
                logger.error("Error with custom room logic: " + error);
            }
            // Set the Simulation Interval callback
            this.setSimulationInterval(dt => {
                this.serverTime += dt;
                //Run Custom Logic for room if loaded
                try {
                    if (customLogic != null)
                        customLogic.ProcessLogic(this, dt);
                }
                catch (error) {
                    logger.error("Error with custom room logic: " + error);
                }
            });
        });
    }
    // Callback when a client has joined the room
    onJoin(client, options) {
        logger.info(`Client joined!- ${client.sessionId} ***`);
        let newNetworkedUser = new ColyseusRoomState_1.ColyseusNetworkedUser().assign({
            id: client.id,
            sessionId: client.sessionId,
        });
        this.state.networkedUsers.set(client.sessionId, newNetworkedUser);
        client.send("onJoin", { newNetworkedUser: newNetworkedUser, customLogic: this.roomOptions["logic"] });
        if (this.customMethodController != null) {
            if (this.customMethodController.ProcessUserJoined != null)
                this.customMethodController.ProcessUserJoined(this, client);
        }
    }
    /**
     * Set the attribute of an entity or a user
     * @param {*} client
     * @param {*} attributeUpdateMessage
     */
    setAttribute(client, attributeUpdateMessage) {
        if (attributeUpdateMessage == null
            || (attributeUpdateMessage.entityId == null && attributeUpdateMessage.userId == null)
            || attributeUpdateMessage.attributesToSet == null) {
            return; // Invalid Attribute Update Message
        }
        // Set entity attribute
        if (attributeUpdateMessage.entityId) {
            //Check if this client owns the object
            if (this.state.networkedEntities.has(`${attributeUpdateMessage.entityId}`) === false)
                return;
            this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).timestamp = parseFloat(this.serverTime.toString());
            let entityAttributes = this.state.networkedEntities.get(`${attributeUpdateMessage.entityId}`).attributes;
            for (let index = 0; index < Object.keys(attributeUpdateMessage.attributesToSet).length; index++) {
                let key = Object.keys(attributeUpdateMessage.attributesToSet)[index];
                let value = attributeUpdateMessage.attributesToSet[key];
                entityAttributes.set(key, value);
            }
        }
        // Set user attribute
        else if (attributeUpdateMessage.userId) {
            //Check is this client ownes the object
            if (this.state.networkedUsers.has(`${attributeUpdateMessage.userId}`) === false) {
                logger.error(`Set Attribute - User Attribute - Room does not have networked user with Id - \"${attributeUpdateMessage.userId}\"`);
                return;
            }
            this.state.networkedUsers.get(`${attributeUpdateMessage.userId}`).timestamp = parseFloat(this.serverTime.toString());
            let userAttributes = this.state.networkedUsers.get(`${attributeUpdateMessage.userId}`).attributes;
            for (let index = 0; index < Object.keys(attributeUpdateMessage.attributesToSet).length; index++) {
                let key = Object.keys(attributeUpdateMessage.attributesToSet)[index];
                let value = attributeUpdateMessage.attributesToSet[key];
                userAttributes.set(key, value);
            }
        }
    }
    // Callback when a client has left the room
    onLeave(client, consented) {
        return __awaiter(this, void 0, void 0, function* () {
            let networkedUser = this.state.networkedUsers.get(client.sessionId);
            if (networkedUser) {
                networkedUser.connected = false;
            }
            logger.silly(`*** User Leave - ${client.sessionId} ***`);
            // this.clientEntities is keyed by client.id
            // this.state.networkedUsers is keyed by client.sessionid
            try {
                if (consented) {
                    throw new Error("consented leave!");
                }
                logger.info("let's wait for reconnection for client: " + client.sessionId);
                const newClient = yield this.allowReconnection(client, 10);
                logger.info("reconnected! client: " + newClient.id);
            }
            catch (e) {
                logger.info("disconnected! client: " + client.id);
                logger.silly(`*** Removing Networked User and Entity ${client.id} ***`);
                //remove user
                this.state.networkedUsers.delete(client.sessionId);
                //remove entites
                if (this.clientEntities.has(client.id)) {
                    let allClientEntities = this.clientEntities.get(client.id);
                    allClientEntities.forEach(element => {
                        this.state.networkedEntities.delete(element);
                    });
                    // remove the client from clientEntities
                    this.clientEntities.delete(client.id);
                    if (this.customMethodController != null) {
                        if (this.customMethodController.ProcessUserLeft != null)
                            this.customMethodController.ProcessUserLeft(this, client);
                    }
                }
                else {
                    logger.error(`Can't remove entities for ${client.id} - No entry in Client Entities!`);
                }
            }
        });
    }
    onDispose() {
    }
}
exports.StarBossRoom = StarBossRoom;
