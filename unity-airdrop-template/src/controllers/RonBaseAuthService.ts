import Logger from "./Logger.ts";
import ApiService, {JwtData} from "./ApiService.ts";
import EncryptedStorageService from "./EncryptedStorageService.ts";
import {decodeJwt} from "jose";
import {AsyncTask, SimpleIntervalJob, ToadScheduler} from "toad-scheduler";
import {getStorageSettings, StorageSettings} from "../consts/Settings.ts";
import VersionManager from "./VersionManager.ts";
import {toNumberOrZero} from "../utils/Number.ts";
import {getRpc, getSupportedNetworkFromChainId} from "./RpcNetworkUtils.ts";
import {WalletId, WalletIdUtils} from "./WalletIdUtils.ts";
import {IAuthService, JwtDataSenspark, JwtDataWallet, JwtValidation, ScheduleBy} from "./IAuthService.ts";
import RonBaseWalletService from "./RonBaseWalletService.ts";
import {HandshakeType} from "./WalletService.ts";
import LoginModal from '../components/LoginModal';
import {getWalletAddress} from "./WalletUtils.ts";
import {getBrowserProvider} from "./BlockChain/Module/Utils/Storage.ts";
import {notificationService} from "../hooks/GlobalServices.ts";
import {sleep} from "../utils/Time.ts";
import { appKitButtonAtom } from '../components/AppKitButtonAtom';
import { getDefaultStore } from 'jotai';

const TAG = '[AUT]';
const K_SINGLE_REFRESH_TOKEN_ID_JOB = "refreshJwt";
const REFRESH_INTERVAL_SECONDS_TEST = 15;
const REFRESH_INTERVAL_SECONDS_PROD = 5 * 60;

export default class RonBaseAuthService implements IAuthService {
    constructor(
        private readonly _isProd: boolean,
        private readonly _storage: EncryptedStorageService,
        private readonly _walletService: RonBaseWalletService,
        private readonly _logger: Logger,
        private readonly _versionManager: VersionManager,
        private readonly _apiService: ApiService,
    ) {
        this._refreshTokenIntervalSeconds = _isProd ? REFRESH_INTERVAL_SECONDS_PROD : REFRESH_INTERVAL_SECONDS_TEST;
        this._storageSettings = getStorageSettings(_isProd);
        this._savedWalletId = WalletIdUtils.getWalletId(this._isProd);
    }

    getJwtForAccount(): Promise<JwtDataSenspark | null> {
        throw new Error("Method not implemented.");
    }

    async changeNickName(userName: string, newNickName: string): Promise<boolean> {
        this._logger.log(`${TAG} change nick name`);
        return await this._apiService.changeNickName(userName, newNickName);
    }

    private readonly _scheduler = new ToadScheduler();
    private readonly _refreshTokenIntervalSeconds: number;
    private readonly _storageSettings: StorageSettings;

    private _scheduleJob: SimpleIntervalJob | null = null;
    private _serverPublicKey: string | null = null;
    private _currentScheduleBy: ScheduleBy = 'none';
    private _refreshToken: string | undefined = undefined;
    private _savedWalletId: WalletId | undefined = undefined;

    /**
     * Nếu return null thì show error message
     */
    async getJwt(type: HandshakeType): Promise<JwtDataWallet | null> {
        // Unity luôn sẽ gọi handshake và lấy jwt, việc đảm bảo có connect ví chưa là của react
        // React check ở đây nếu chưa thì video bg đã show rồi, giờ chỉ đợi user connect ví
        while (!this._walletService.getConnection()) {
            this._logger.log(`${TAG} Waiting for wallet connection...`);
            await sleep(500);
        }

        // reconnect thì auto cho login
        const canAutoLogin = type === "Reconnect";

        const autoLoginAgain = async (autoLogin: boolean) => {
            let sure = false;
            while (!sure) {
                if (autoLogin) {
                    break;
                }
                const network = getRpc(this._walletService.currentNetworkType!, this._isProd);
                if (!network) {
                    this._logger.error(`${TAG} No network found for current network type`);
                    return null;
                }
                const selectedWallet = await this._walletService.getWalletAddress();
                if (!selectedWallet) {
                    this._logger.error(`${TAG} No wallet address selected`);
                    return null;
                }

                if (!canAutoLogin) {
                    LoginModal.open(selectedWallet, network.chainName);
                    const result = await LoginModal.waitForUser();
                    // Show custom AppKitButton UI after user interaction
                    if (result.address && result.network) {
                        getDefaultStore().set(appKitButtonAtom, { showCustomUI: true, network: result.network, address: result.address });
                    }
                    if (!result.address || !result.network) {
                        this._logger.error(`${TAG} User cancelled login or missing info`);
                        return null;
                    }

                    if (result.hasChange) {
                        // Kiểm tra wallet có đúng là wallet đã chọn ko
                        const addressInMetaMask = await getWalletAddress(this._logger, getBrowserProvider());
                        if (!addressInMetaMask) {
                            notificationService.showError("Error happened. Please check your wallet connection.");
                            return null;
                        }
                        // Chỗ này do user đổi ví trên app-kit nhưng ko thể trực tiếp gọi metamassk để dổi ví dùm user đc
                        // Nên sẽ thông báo lỗi, user phải tự mở metamask để đổi ví
                        if (addressInMetaMask.toLowerCase() !== result.address.toLowerCase()) {
                            this._logger.error(`${TAG} Wallet address mismatch: expected ${result.address}, got ${addressInMetaMask}`);
                            notificationService.showError("Wallet address mismatch. Please connect correct wallet.");
                            getDefaultStore().set(appKitButtonAtom, { showCustomUI: false });
                            return null;
                        }
                    }
                }
                sure = true;
            }
            return await this.loginWithWallet();
        };
        
        const canUseOldData = async(): Promise<boolean> =>{
            const hasAddress = await this._walletService.getWalletAddress()
            if(!hasAddress) {
                return false
            }
            const hasChainId = await this._walletService.getChainId();
            if(!hasChainId){
                return false;
            }
            return true;
        
        }

        try {
            // Lấy account đã từng login trước
            const walletId = this._savedWalletId;
            if (!walletId) {
                // Need login again
                this._logger.error(`${TAG} No walletId found, need to login again`);
                return await autoLoginAgain(false);
            }
            
            //Nếu có walletId mà walletAddress hoặc chainId ko có thì có thể do user logout hoặc bị lỗi gì đó
            // nền cần thử update lại từ metamask nếu ko đc thì thôi
            const canLogin = await canUseOldData()
            if(!canLogin) {            
                await this._walletService.tryGetProfileFromAppkit()
            }            
            
            const jwt = await this.getJwtFromStorage(walletId.fullId);
            this._refreshToken = jwt?.ref;

            this._logger.log(`${TAG} walletId=${walletId.fullId} jwt=${jwt?.jwt}`);
            const version = this._versionManager.getCurrentVersion();
            this._logger.log(`${TAG} version: ${version}`);
            const jwtValidation = this.validateJwt(jwt?.jwt, walletId.wallet);
            this._logger.log(`${TAG} jwtValidation: ${jwtValidation}`);

            const willLoginAgain = jwtValidation != JwtValidation.Valid;

            const applyWallet = (walletId: WalletId, jwt: string, version: number): JwtDataWallet => {
                this._apiService.setApiHost(walletId.network);
                this._walletService.setChainId(walletId.chainId);
                this._walletService.updateWalletAddress(walletId.wallet);
                this._walletService.stopUpdateWalletInfo();
                this.scheduleRefreshJwt('wallet');

                // Đảm bảo url đc set lại nếu user chọn reset ở bước login moodal
                this._savedWalletId = WalletIdUtils.saveWalletId(walletId.wallet, walletId.chainId, this._isProd);

                return {
                    walletAddress: walletId.wallet!,
                    jwt: jwt!,
                    version: version,
                }
            };
            const chainName = walletId.rpc.chainName;
            // let modalTitle = `Connect to ${chainName}?`;

            // Nếu jwt valid hoặc jwt từng valid nhưng đã bị expired, thì cho phép hỏi lại, nhưng nếu là reconnect thì cho login luôn
            if (
                (jwtValidation === JwtValidation.Valid) ||
                (willLoginAgain && jwtValidation === JwtValidation.Expired)
            ) {
                // Hỏi user có muốn tiếp tục dùng wallet đã lưu trước đó hay không
                let yes = true;
                // canAutoLogin tức là user đã confirm rồi hoặc đây là reconnect => ko hỏi lại
                if (!canAutoLogin) {
                    LoginModal.open(walletId.wallet, chainName, true);
                    const result = await LoginModal.waitForUser();
                    if (!result.address || !result.network) {
                        this._logger.error(`${TAG} User cancelled login or missing info`);
                        return null;
                    }

                    // Show custom AppKitButton UI after user interaction
                    getDefaultStore().set(appKitButtonAtom, { showCustomUI: true, network: result.network, address: result.address });
                    
                    // user đã xác nhận đổi profile trong modal rồi, cho login luôn, ko hỏi lại
                    if (result.hasChange) {
                        WalletIdUtils.removeWalletId()
                        // Kiểm tra wallet có đúng là wallet đã chọn ko
                        const addressInMetaMask = await getWalletAddress(this._logger, getBrowserProvider());
                        if (!addressInMetaMask) {
                            notificationService.showError("Error happened. Please check your wallet connection.");
                            return null;
                        }
                        // Chỗ này do user đổi ví trên app-kit nhưng ko thể trực tiếp gọi metamassk để dổi ví dùm user đc
                        // Nên sẽ thông báo lỗi, user phải tự mở metamask để đổi ví
                        if (addressInMetaMask.toLowerCase() !== result.address.toLowerCase()) {
                            this._logger.error(`${TAG} Wallet address mismatch: expected ${result.address}, got ${addressInMetaMask}`);
                            notificationService.showError("Wallet address mismatch. Please connect correct wallet.");
                            getDefaultStore().set(appKitButtonAtom, { showCustomUI: false });
                            return null;
                        }
                        return await autoLoginAgain(true)
                    }
                }
                yes = true;
                if (yes) {
                    if (jwtValidation === JwtValidation.Valid) {
                        this._logger.log(`${TAG} jwt is valid`);
                        return applyWallet(walletId, jwt!.jwt, version);
                    }
                    if (jwtValidation === JwtValidation.Expired) {
                        this._logger.log(`${TAG} jwt is expired`);
                        const newJwt = await this.refreshJwt();
                        if (newJwt) {
                            this._logger.log(`${TAG} version is updated to ${newJwt.version}`);
                            return applyWallet(walletId, newJwt.jwt, newJwt.version!);
                        }
                    }
                }
            }
            // auto login again
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            // auto login again
        }

        return await autoLoginAgain(canAutoLogin);
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
        WalletIdUtils.removeWalletId();
        await this._walletService.logOut();
        this.unScheduleRefreshJwt(this._currentScheduleBy);
        // Reset LoginModal skip flag so modal can show again after logout
        LoginModal.resetSkipModal();
    }


    private async loginWithWallet(): Promise<JwtDataWallet | null> {
        try {
            this._logger.log(`${TAG} login again`);
            this.unScheduleRefreshJwt('wallet');
            await this._walletService.connectWallet();
            const walletAddress = await this._walletService.getWalletAddress();
            const chainId = this._walletService.getChainId();
            if (!walletAddress || !chainId) {
                this._logger.error(`${TAG} Wallet address (${walletAddress}) or chain ID (${chainId}) is not available`);
                return null;
            }
            const supportedNetwork = getSupportedNetworkFromChainId(chainId.dec);
            if (!supportedNetwork) {
                this._logger.error(`${TAG} Unsupported chain ID: ${chainId.dec}`);
                return null;
            }
            this._apiService.setApiHost(supportedNetwork);
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
            this._refreshToken = jwtRes.refreshToken;
            this.scheduleRefreshJwt('wallet');
            const extraData = JSON.parse(jwtRes.extraData) as { version: string };

            // assign wallet id
            this._savedWalletId = WalletIdUtils.saveWalletId(walletAddress, chainId, this._isProd);
            await this.setJwtIntoStorage(this._savedWalletId.fullId, jwtRes);
            this._walletService.stopUpdateWalletInfo();

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

    /**
     * Refresh every 5 minutes
     */
    public scheduleRefreshJwt(type: ScheduleBy): void {
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
        if (!this._refreshToken) {
            this._logger.error(`${TAG} No refresh token found`);
            return null;
        }
        this._logger.log(`${TAG} start refresh jwt`);
        const jwtRes = await this._apiService.refreshJwtToken(this._refreshToken);
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

        if (!this._savedWalletId) {
            // should not happened
            this._logger.log(`${TAG} does not have savedWalletId`);
            return null;
        }

        this._serverPublicKey = jwtRes.key;
        this._logger.log(`${TAG} serverPublicKey: ${this._serverPublicKey}`);
        jwtRes.refreshToken = this._refreshToken; // bổ sung thêm
        await this.setJwtIntoStorage(this._savedWalletId.fullId, jwtRes);

        return {
            walletAddress: this._savedWalletId.wallet,
            jwt: jwtRes.jwt,
            version: versionFromApi,
        };
    }

    private validateJwt(jwt: string | undefined, savedWalletAddress: string | undefined): JwtValidation {
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
            let jwtAddress = decoded.address as string;
            if (!jwtAddress) {
                //account
                jwtAddress = decoded.userName as string;
            }
            if (!jwtAddress) {
                this._logger.error(`Error: Missing address or userName`);
                return JwtValidation.NoAddress;
            }
            if (jwtAddress.toLowerCase() != savedWalletAddress.toLowerCase()) {
                this._logger.error(`Error: Wallet address in jwt (${jwtAddress}) does not match with saved wallet address ${savedWalletAddress}`);
                return JwtValidation.NoAddress;
            }
            return JwtValidation.Valid;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return JwtValidation.Error;
        }
    }

    private async getJwtFromStorage(walletId: string): Promise<JwtStorageData | null> {
        const key = `${this._storageSettings.keyJwt}/${walletId}`;
        const val = await this._storage.get(key);
        this._logger.log(`${TAG} Get Storage token: ${key}=${val}`);
        if (!val) {
            this._logger.error(`${TAG} No jwt found in storage for walletId: ${walletId}`);
            return null;
        }
        const parsed = JSON.parse(val) as JwtStorageData;
        if (!parsed || !parsed.jwt || !parsed.ref) {
            this._logger.error(`${TAG} Invalid jwt data in storage for walletId: ${walletId}`);
            return null;
        }
        return parsed;
    }

    private async setJwtIntoStorage(walletId: string, jwt: JwtData): Promise<void> {
        const key = `${this._storageSettings.keyJwt}/${walletId}`;
        this._logger.log(`${TAG} Set Storage token: ${key}=${jwt}`);
        const value = JSON.stringify({
            jwt: jwt.jwt,
            ref: jwt.refreshToken,
        } satisfies  JwtStorageData);
        await this._storage.set(key, value);
    }
}

type JwtStorageData = {
    jwt: string; // jwt
    ref: string; // refresh token
};