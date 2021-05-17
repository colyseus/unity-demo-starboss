
export class StarBossWorm {

    health: number;

    constructor(startingHealth: number) {
        
        if(isNaN(Number(startingHealth))) {
            throw `Error - StarBossWorm - Invalid health = ${startingHealth}`;
        }

        this.health = Number(startingHealth);
    }
};
