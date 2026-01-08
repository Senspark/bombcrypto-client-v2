import Logger from "./Logger.ts";
import ApiService from "./ApiService.ts";
import EncryptedStorageService from "./EncryptedStorageService.ts";
import {decodeJwt} from "jose";
import WalletService from "./WalletService.ts";
import {AsyncTask, SimpleIntervalJob, ToadScheduler} from "toad-scheduler";
import {getStorageSettings, StorageSettings} from "../consts/Settings.ts";
import VersionManager from "./VersionManager.ts";


const TAG = '[AUT]';
const K_SINGLE_REFRESH_TOKEN_ID_JOB = "refreshJwt";
const REFRESH_INTERVAL_SECONDS_TEST = 60;
const REFRESH_INTERVAL_SECONDS_PROD = 5 * 60;

export default class AuthService {
    constructor(
        isProd: boolean,
        private readonly _storage: EncryptedStorageService,
        private readonly _walletService: WalletService,
        private readonly _logger: Logger,
        private readonly _versionManager: VersionManager,
        private readonly _apiService: ApiService,

    ) {
        this._refreshTokenIntervalSeconds = isProd ? REFRESH_INTERVAL_SECONDS_PROD : REFRESH_INTERVAL_SECONDS_TEST;
        this._storageSettings = getStorageSettings(isProd);
        this._walletService.setOnWalletDisconnected(this.onWalletDisconnectWallet.bind(this));
    }
    
    private readonly _scheduler = new ToadScheduler();
    private readonly _refreshTokenIntervalSeconds: number;
    private readonly _storageSettings: StorageSettings;

    private _scheduleJob: SimpleIntervalJob | null = null;
    private _serverPublicKey: string | null = null;

    /**
     * Nếu return null thì show error message
     */
    async getJwt(): Promise<JwtData | null> {
        try {
            
            const settings = this._storageSettings;
            const jwt = await this._storage.get(settings.keyJwt);
            this._logger.log(`${TAG} jwt: ${jwt}`);
            const version = this._versionManager.getCurrentVersion();
            this._logger.log(`${TAG} version: ${version}`);
            const savedWalletAddress = await this._storage.get(settings.keyWalletAddress);
            this._logger.log(`${TAG} addr1: ${savedWalletAddress}`);
            const jwtValidation = this.validateJwt(jwt, savedWalletAddress);
            this._logger.log(`${TAG} jwtValidation: ${jwtValidation} savedWalletAddress ${savedWalletAddress}`);

            if (jwtValidation === JwtValidation.Valid) {
                this._logger.log(`${TAG} jwt is valid`);
                this.scheduleRefreshJwt();
                return {
                    walletAddress: savedWalletAddress!,
                    jwt: jwt!,
                    version: version,
                }
            }

            if (jwtValidation === JwtValidation.Expired) {
                this._logger.log(`${TAG} jwt is expired`);
                const newJwt = await this.refreshJwt();
                if (newJwt) {
                    this._logger.log(`${TAG} version is updated to ${newJwt.version}`);
                    this.scheduleRefreshJwt();
                    return {
                        walletAddress: savedWalletAddress!,
                        jwt: newJwt.jwt,
                        version: newJwt.version, // Lấy version mới từ api về gửi cho client
                    }
                }
                // must login again
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
        }

        return await this.loginAgain();
    }

    async getServerPublicKey(): Promise<string | null> {
        this._logger.log(`${TAG} getServerPublicKey`);
        if (!this._serverPublicKey) {
            const success = await this.refreshJwt();
            if (!success) {
                this._logger.error(`Error: Cannot get server public key`);
                return null;
            }
        }
        const pKey = this._serverPublicKey;
        this._logger.log(`${TAG} serverPublicKey: ${pKey}`);
        return pKey;
    }

    async logout() {
        this._logger.log(`${TAG} user logout`);
        await this._walletService.disconnectWallet();
    }

    private onWalletDisconnectWallet() {
        this._logger.log(`${TAG} clear wallet data`);
        this.unScheduleRefreshJwt();
        this.clearAllData();
    }

    private async loginAgain(): Promise<JwtData | null> {
        try {
            this._logger.log(`${TAG} login again`);
            this.unScheduleRefreshJwt();
            this.clearAllData();
            const walletAddress = await this._walletService.getWalletAddress();
            const nonce = await this._apiService.getNonce(walletAddress);
            if (!nonce) {
                this._logger.error(`Error: Cannot get nonce`);
                return null;
            }
            const signature = await this._walletService.sign(nonce);
            if (!signature) {
                this._logger.error(`Error: Cannot sign`);
                return null;
            }
            const jwtRes = await this._apiService.checkProofAndGetJwtToken(walletAddress, signature);
            if (!jwtRes) {
                this._logger.error(`Error: Cannot get jwt`);
                return null;
            }
            this._serverPublicKey = jwtRes.key;
            const settings = this._storageSettings;
            await this._storage.set(settings.keyJwt, jwtRes.jwt);
            await this._storage.set(settings.keyWalletAddress, walletAddress);
            this.scheduleRefreshJwt();
            return {
                walletAddress: walletAddress,
                jwt: jwtRes.jwt,
                version: jwtRes.version,
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    private clearAllData() {
        this._logger.log(`${TAG} clear all data`);
        const settings = this._storageSettings;
        this._storage.remove(settings.keyJwt);
        this._storage.remove(settings.keyWalletAddress);
    }

    /**
     * Refresh every 5 minutes
     */
    private scheduleRefreshJwt(): void {
        this.unScheduleRefreshJwt();
        this._logger.log(`${TAG} schedule refresh jwt`);

        const task = new AsyncTask('refreshJwt', () => this.refreshJwt().then(), (e: Error) => {
            this._logger.error(`Error: ${e.message}`);
        });
        this._scheduleJob = new SimpleIntervalJob({seconds: this._refreshTokenIntervalSeconds}, task, {
            id: K_SINGLE_REFRESH_TOKEN_ID_JOB,
            preventOverrun: true,
        });
        this._scheduler.addSimpleIntervalJob(this._scheduleJob);
    }

    private unScheduleRefreshJwt(): void {
        if (this._scheduleJob) {
            this._logger.log(`${TAG} unschedule refresh jwt`);
            const id = this._scheduleJob.id;
            if (id) {
                this._scheduler.stopById(id);
                this._scheduler.removeById(id);
            }
            this._scheduleJob.stop();
        }
        this._scheduleJob = null;
    }

    private async refreshJwt(): Promise<JwtData|null> {
        this._logger.log(`${TAG} start refresh jwt`);
        const jwtRes = await this._apiService.refreshJwtToken();
        if (!jwtRes) {
            this._logger.error(`Error: Cannot refresh jwt`);
            return null;
        }
        
        const isValidVersion = await this._versionManager.checkVersion(jwtRes.version);
        if(!isValidVersion) {
            this._logger.error("Version is not valid");
            return null;
        }
        
        const settings = this._storageSettings;
        this._serverPublicKey = jwtRes.key;
        await this._storage.set(settings.keyJwt, jwtRes.jwt);
        const savedWalletAddress = await this._storage.get(settings.keyWalletAddress);

        return {
            walletAddress: savedWalletAddress!,
            jwt: jwtRes.jwt,
            version: jwtRes.version,
        };
    }

    private validateJwt(jwt: string | null, savedWalletAddress: string | null): JwtValidation {
        if (!jwt) {
            this._logger.error(`Error: Missing jwt`);
            return JwtValidation.Error;
        }
        if (!savedWalletAddress) {
            this._logger.error(`Error: Missing saved wallet address`);
            return JwtValidation.Error;
        }
        try {
            // check if jwt is not expired
            const decoded = decodeJwt(jwt);
            const exp = decoded.exp;
            if (!exp) {
                this._logger.error(`Error: Missing exp`);
                return JwtValidation.Error;
            }
            const now = Math.floor(Date.now() / 1000);
            // jwt must at least 5 minutes before expired
            if (exp - now < 300) {
                this._logger.error(`Error: Jwt is expired`);
                return JwtValidation.Expired;
            }
            const jwtWalletAddress = decoded.address as string;
            if (!jwtWalletAddress) {
                this._logger.error(`Error: Missing address`);
                return JwtValidation.NoAddress;
            }
            if (jwtWalletAddress.toLowerCase() != savedWalletAddress.toLowerCase()) {
                this._logger.error(`Error: Wallet address in jwt is not match with saved wallet address`);
                return JwtValidation.NoAddress;
            }
            return JwtValidation.Valid;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return JwtValidation.Error;
        }
    }
}

enum JwtValidation {
    Error, Expired, NoAddress, Valid
}

export type JwtData = {
    walletAddress: string,
    jwt: string,
    version: number | null,
}