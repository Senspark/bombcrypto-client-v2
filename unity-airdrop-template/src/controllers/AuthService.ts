import Logger from "./Logger.ts";
import ApiService from "./ApiService.ts";
import EncryptedStorageService from "./EncryptedStorageService.ts";
import {decodeJwt} from "jose";
import WalletService, {Account, HandshakeType} from "./WalletService.ts";
import {AsyncTask, SimpleIntervalJob, ToadScheduler} from "toad-scheduler";
import {getStorageSettings, StorageSettings} from "../consts/Settings.ts";
import VersionManager from "./VersionManager.ts";
import {toNumberOrZero} from "../utils/Number.ts";
import {IAuthService, JwtDataSenspark, JwtValidation, ScheduleBy, JwtDataWallet} from "./IAuthService.ts";

const TAG = '[AUT]';
const K_SINGLE_REFRESH_TOKEN_ID_JOB = "refreshJwt";
const REFRESH_INTERVAL_SECONDS_TEST = 60;
const REFRESH_INTERVAL_SECONDS_PROD = 5 * 60;

export default class AuthService implements IAuthService {
    constructor(
        isProd: boolean,
        private readonly _storage: EncryptedStorageService,
        private readonly _walletService: WalletService,
        private readonly _logger: Logger,
        private readonly _versionManager: VersionManager,
        private readonly _apiService: ApiService
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
    private _needCheckProofAccountAgain: boolean = false;
    private _currentScheduleBy: ScheduleBy = 'none';

    /**
     * Nếu return null thì show error message
     */
    async getJwt(type: HandshakeType): Promise<JwtDataWallet | null> {
        try {
            this._logger.log(`Handshake type is ${type}`)
            const settings = this._storageSettings;
            const jwt = await this._storage.get(settings.keyJwt);
            this._logger.log(`${TAG} jwt: ${jwt}`);
            const version = this._versionManager.getCurrentVersion();
            this._logger.log(`${TAG} version: ${version}`);
            const savedWalletAddress = await this._storage.get(settings.keyWalletAddress);
            this._logger.log(`${TAG} addr1: ${savedWalletAddress}`);
            const jwtValidation = this.validateJwt(jwt, savedWalletAddress);
            this._logger.log(`${TAG} jwtValidation: ${jwtValidation} savedWalletAddress ${savedWalletAddress}`);

            // Set apiHost liền trong trường hợp là Ronin hoặc Base
            this._apiService.setApiHost(this._walletService.currentNetworkType);

            // User login bằng 1 account senspark khác
            const currentWallet = this._walletService.getWalletAddress();
            if (!currentWallet && (currentWallet !== savedWalletAddress)) {
                this._logger.log(`${TAG} account change, need to login again`);
                // Login again
            }

            if (jwtValidation === JwtValidation.Valid) {
                this._logger.log(`${TAG} jwt is valid`);
                this.scheduleRefreshJwt('wallet');
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
                    this.scheduleRefreshJwt('wallet');
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

        return await this.loginWithWallet();
    }

    async getJwtForAccount(account: Account): Promise<JwtDataSenspark | null> {
        try {
            const settings = this._storageSettings;
            const jwt = await this._storage.get(settings.keyJwt);
            this._logger.log(`${TAG} jwt: ${jwt}`);
            const version = this._versionManager.getCurrentVersion();
            this._logger.log(`${TAG} version: ${version}`);
            const savedWalletAddress = await this._storage.get(settings.keyWalletAddress);
            this._logger.log(`${TAG} addr1: ${savedWalletAddress}`);
            const jwtValidation = this.validateJwt(jwt, savedWalletAddress);
            this._logger.log(`${TAG} jwtValidation: ${jwtValidation} saveUserName ${savedWalletAddress}`);
            const isUserFi = await this._storage.get(settings.userFi) || "false";

            //User login bằng 1 account senspark khác
            if (account.userName !== savedWalletAddress) {
                this._logger.log(`${TAG} account change, need to login again`);
                //login again
            }

            //User login bằng account cũ nhưng do đã reload lại trang web rồi nên cần check user pass lại
            else if (this._needCheckProofAccountAgain) {
                this._needCheckProofAccountAgain = false;
                this._logger.log(`${TAG} need to check proof account again`);
                //login again
            }

            // User reconnect
            else if (jwtValidation === JwtValidation.Valid) {
                this._logger.log(`${TAG} jwt is valid`);
                this.scheduleRefreshJwt('account');
                return {
                    address: savedWalletAddress!,
                    jwt: jwt!,
                    version: version,
                    isUserFi: isUserFi === "true"
                }
            } else if (jwtValidation === JwtValidation.Expired) {
                this._logger.log(`${TAG} jwt is expired`);
                const newJwt = await this.refreshJwt();
                if (newJwt) {
                    this._logger.log(`${TAG} version is updated to ${newJwt.walletAddress}`);
                    this.scheduleRefreshJwt('account');
                    return {
                        address: savedWalletAddress!,
                        jwt: newJwt.jwt,
                        version: newJwt.version,
                        isUserFi: isUserFi === "true"
                    }
                }
                // must login again
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
        }

        return await this.loginWithAccount(account);
    }

    async changeNickName(userName: string, newNickName: string): Promise<boolean> {
        this._logger.log(`${TAG} change nick name`);
        return await this._apiService.changeNickName(userName, newNickName);
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
        window.setConnectWallet(true);
        window.setConnectUser(false);
        window.setUserName('');
        await this._walletService.disconnectWallet();
        this.unScheduleRefreshJwt(this._currentScheduleBy);
        this.clearAllData();
        this._needCheckProofAccountAgain = true;
    }

    changeAccount() {
        this._logger.log(`${TAG} user change account`);
        this.unScheduleRefreshJwt('wallet');
        this.clearAllData();
    }

    private onWalletDisconnectWallet() {
        this._logger.log(`${TAG} clear wallet data`);
        this.unScheduleRefreshJwt('wallet');
        this.clearAllData();
    }

    private async loginWithWallet(): Promise<JwtDataWallet | null> {
        try {
            this._logger.log(`${TAG} login again`);
            this.unScheduleRefreshJwt('wallet');
            this.clearAllData();
            const walletAddress = await this._walletService.getWalletAddress();
            this._apiService.setApiHost(this._walletService.currentNetworkType);
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
            this.scheduleRefreshJwt('wallet');
            const extraData = JSON.parse(jwtRes.extraData) as { version: string };
            return {
                walletAddress: walletAddress,
                jwt: jwtRes.jwt,
                version: toNumberOrZero(extraData.version),
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    private async loginWithAccount(account: Account): Promise<JwtDataSenspark | null> {
        try {
            this._logger.log(`${TAG} login again with password`);
            this.unScheduleRefreshJwt('account');
            this.clearAllData();
            if (!account) {
                this._logger.error(`${TAG} Account is null`);
                return null;
            }
            const jwtRes = await this._apiService.checkProofForAccount(account);
            if (!jwtRes) {
                this._logger.error(`Error: Cannot get jwt`);
                return null;
            }
            this._serverPublicKey = jwtRes.key;
            const settings = this._storageSettings;

            this.scheduleRefreshJwt('account');
            const extraData = JSON.parse(jwtRes.extraData) as {
                version: string,
                isUserFi: boolean,
                address: string
            };
            await this._storage.set(settings.keyJwt, jwtRes.jwt);
            await this._storage.set(settings.keyWalletAddress, account.userName);
            await this._storage.set(settings.userFi, extraData.isUserFi.toString());

            const versionFromApi = toNumberOrZero(extraData.version);
            const isValidVersion = await this._versionManager.checkVersion(versionFromApi);

            if (!isValidVersion) {
                this._logger.error("Version is not valid");
                return null;
            }

            return {
                jwt: jwtRes.jwt,
                version: toNumberOrZero(extraData.version),
                isUserFi: extraData.isUserFi,
                address: extraData.address || null
            }
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }


    private clearAllData() {
        this._logger.log(`${TAG} clear all data`);
        window.setLoginState('fetching');
        const settings = this._storageSettings;
        this._storage.remove(settings.keyJwt);
        this._storage.remove(settings.keyWalletAddress);
        this._storage.remove(settings.userFi);
    }

    /**
     * Refresh every 5 minutes
     */
    private scheduleRefreshJwt(type: ScheduleBy): void {
        this.unScheduleRefreshJwt(this._currentScheduleBy);
        if (this._currentScheduleBy !== 'none') {
            return;
        }
        this._currentScheduleBy = type;

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

    private unScheduleRefreshJwt(type: ScheduleBy): void {
        if (this._currentScheduleBy !== type) {
            //Chỉ đc unschedule khi cùng loại đã schedule
            return;
        }
        this._currentScheduleBy = 'none';
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

    private async refreshJwt(): Promise<JwtDataWallet | null> {
        this._logger.log(`${TAG} start refresh jwt`);
        const jwtRes = await this._apiService.refreshJwtToken("");
        if (!jwtRes) {
            this._logger.error(`Error: Cannot refresh jwt`);
            return null;
        }
        const versionJson = JSON.parse(jwtRes.extraData) as { version: string };
        const versionFromApi = toNumberOrZero(versionJson.version);
        const isValidVersion = await this._versionManager.checkVersion(versionFromApi);

        if (!isValidVersion) {
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
            version: versionFromApi,
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
            //wallet
            let address = decoded.address as string;
            if (!address) {
                //account
                address = decoded.userName as string;
            }
            if (!address) {
                this._logger.error(`Error: Missing address or userName`);
                return JwtValidation.NoAddress;
            }
            if (address.toLowerCase() != savedWalletAddress.toLowerCase()) {
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