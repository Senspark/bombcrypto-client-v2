import {EnvConfig} from "../configs/EnvConfig.ts";

class GameConstant {
    static getGameLink(version: string): string {
        //Production (chỉ prod mới cần update version)
        if(EnvConfig.isProduction() && !EnvConfig.isMainTest())
            return `https://game.bombcrypto.io/solana/v${version}/index.html`;
        
        //Main Test
        if(EnvConfig.isProduction() && EnvConfig.isMainTest())
            return ``;

        //Test
        if(EnvConfig.isGcloud())
            return ``;      
        
        //Local
        else
            return `http://localhost:5173/`;
    }
}

export default GameConstant;