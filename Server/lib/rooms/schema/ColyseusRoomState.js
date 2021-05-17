"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.ColyseusRoomState = exports.ColyseusNetworkedUser = exports.ColyseusNetworkedEntity = void 0;
const schema_1 = require("@colyseus/schema");
class ColyseusNetworkedEntity extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.creationId = "";
        this.xPos = 0;
        this.yPos = 0;
        this.zPos = 0;
        this.xRot = 0;
        this.yRot = 0;
        this.zRot = 0;
        this.wRot = 0;
        this.xScale = 1;
        this.yScale = 1;
        this.zScale = 1;
        this.xVel = 0;
        this.yVel = 0;
        this.zVel = 0;
        this.attributes = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type("string")
], ColyseusNetworkedEntity.prototype, "id", void 0);
__decorate([
    schema_1.type("string")
], ColyseusNetworkedEntity.prototype, "ownerId", void 0);
__decorate([
    schema_1.type("string")
], ColyseusNetworkedEntity.prototype, "creationId", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "xPos", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "yPos", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "zPos", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "xRot", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "yRot", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "zRot", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "wRot", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "xScale", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "yScale", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "zScale", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "xVel", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "yVel", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "zVel", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedEntity.prototype, "timestamp", void 0);
__decorate([
    schema_1.type({ map: "string" })
], ColyseusNetworkedEntity.prototype, "attributes", void 0);
exports.ColyseusNetworkedEntity = ColyseusNetworkedEntity;
class ColyseusNetworkedUser extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.attributes = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type("string")
], ColyseusNetworkedUser.prototype, "id", void 0);
__decorate([
    schema_1.type("string")
], ColyseusNetworkedUser.prototype, "sessionId", void 0);
__decorate([
    schema_1.type("boolean")
], ColyseusNetworkedUser.prototype, "connected", void 0);
__decorate([
    schema_1.type("number")
], ColyseusNetworkedUser.prototype, "timestamp", void 0);
__decorate([
    schema_1.type({ map: "string" })
], ColyseusNetworkedUser.prototype, "attributes", void 0);
exports.ColyseusNetworkedUser = ColyseusNetworkedUser;
class ColyseusRoomState extends schema_1.Schema {
    constructor() {
        super(...arguments);
        this.networkedEntities = new schema_1.MapSchema();
        this.networkedUsers = new schema_1.MapSchema();
        this.attributes = new schema_1.MapSchema();
    }
}
__decorate([
    schema_1.type({ map: ColyseusNetworkedEntity })
], ColyseusRoomState.prototype, "networkedEntities", void 0);
__decorate([
    schema_1.type({ map: ColyseusNetworkedUser })
], ColyseusRoomState.prototype, "networkedUsers", void 0);
__decorate([
    schema_1.type({ map: "string" })
], ColyseusRoomState.prototype, "attributes", void 0);
exports.ColyseusRoomState = ColyseusRoomState;
