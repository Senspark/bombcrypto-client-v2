import NotificationService from "./NotificationService.ts";
import Logger from "./Logger.ts";
import {sleep} from "../utils/Time.ts";
import {EnvConfig} from "../configs/EnvConfig.ts";
import {toNumberOrZero} from "../utils/Number.ts";

const TAG = '[VER]';
export default class VersionManager {
    private readonly currentVersion : number;
    private readonly ignoreVersionCheck : boolean;
    
    constructor(
        private readonly _logger: Logger,
        private readonly _notification: NotificationService
    ) {
        this.ignoreVersionCheck = EnvConfig.ignoreCheckVersion();
        this.currentVersion = this.getVersionFromUrl();
    }
    
    async checkVersion(versionFromApi: number | null): Promise<boolean> {
        //Web chưa cần check version
        if(this.ignoreVersionCheck) {
            return true;
        }
        
        const version  = this.currentVersion;
        this._logger.log(`${TAG} Check for new version [Current version]: ${version} [Latest version]: ${versionFromApi}`);
        
        if(version === 0) {
            // Version hiện tại là 0 có thể là lỗi hoặc đã đổi link kiểu mới nên skip update version
            this._logger.error(`${TAG} Current version from url is 0, it can be an error => skip update version, use the current version`);
            return true;
        }
        
        if(versionFromApi == null){
            this._logger.error(`${TAG} Latest version from api is null, skip update version, use the current version`);
            return true;
        }
        
        if (version < versionFromApi) {
            await this.goToNewVersion(versionFromApi);
            return false;
        }
        return true;
    }
    
    getCurrentVersion(): number {
        return this.currentVersion;
    }
    

    private getVersionFromUrl(): number {
        if(this.ignoreVersionCheck)
            return 0;
        
        const url = window.location.href;
        const match = url.match(/web\/v(\d+)/);
        if(!match) {
            this._logger.error(`${TAG} Can not get version from url`);
            return 0;
        }
        return toNumberOrZero(match[1]);
    }

    private async goToNewVersion(version: number) {
        this._notification.show("New version found", "We are updating the new game version, please wait a moment. Thank you for your patience.")
        await sleep(5000);
        window.location.href = `${this.getLinkGame(version)}`;
    }
    
    private getLinkGame(version: number): string {
        if(EnvConfig.isProduction() && !EnvConfig.isMainTest())
            return `https://game.bombcrypto.io/web/v${version}/index.html`;

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