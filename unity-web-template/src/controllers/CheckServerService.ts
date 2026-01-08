
import {atom, getDefaultStore} from "jotai";
import ApiService from "./ApiService.ts";
import {sleep} from "../utils/Time.ts";

export const serverMaintenanceStatus = atom<ServerMaintenanceStatus>('checking');
const LOOP_INTERVAL = 1000 * 60 * 5; // 5 minutes
//const LOOP_INTERVAL = 1000 * 5; // 5 seconds
const store = getDefaultStore();

export default class CheckServerService {
    constructor(
        private readonly _apiHost: string,
        private readonly _api: ApiService,
        private readonly _ignore: boolean,
    ) {
        if (this._ignore) {
            this._isMaintenance = false;
        }
        this.loop().then();
    }

    private _isMaintenance: boolean | null = null;

    public async checkServerMaintenance(): Promise<boolean> {
        const path = `${this._apiHost}/check_server`;
        const res = await this._api.sendGet<boolean>(path);
        this._isMaintenance = res ?? false;
        store.set(serverMaintenanceStatus, this._isMaintenance ? 'maintenance' : 'normal');
        return this._isMaintenance;
    }

    private async loop() {
        const isRunning = true;

        while (isRunning) {
            console.log('Checking server maintenance status...');
            await this.checkServerMaintenance();
            await sleep(LOOP_INTERVAL);
        }
    }
}

export type ServerMaintenanceStatus = 'checking' | 'maintenance' | 'normal';
