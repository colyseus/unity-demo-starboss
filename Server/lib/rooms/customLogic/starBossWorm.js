"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.StarBossWorm = void 0;
class StarBossWorm {
    constructor(startingHealth) {
        if (isNaN(Number(startingHealth))) {
            throw `Error - StarBossWorm - Invalid health = ${startingHealth}`;
        }
        this.health = Number(startingHealth);
    }
}
exports.StarBossWorm = StarBossWorm;
;
