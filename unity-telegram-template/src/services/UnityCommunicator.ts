import {copyToClipboard, sleep} from "./Utils.ts";
import {Address} from "@ton/core";
import {EnvConfig} from "./EnvConfig.ts";
import Logger from "./Logger.ts";
import IObfuscate from "../encrypt/IObfuscate.ts";
import RsaEncryption from "../encrypt/RsaEncryption.ts";
import AesEncryption from "../encrypt/AesEncryption.ts";
import AesEncryptionHelper from "../encrypt/AesEncryptionHelper.ts";
import TonService from "./TonService.ts";
import {authApi, logger} from "./GlobalServices.ts";
import {decodeJwt} from "jose";
import BackendAuthApiService from "../apis/BackendAuthApi.ts";
import CheckServerService from "./CheckServerService.ts";

const TAG = '[COM]';
const K_TRANSFER_TON_PREFIX = 'DEP';
const K_TRANSFER_BCOIN_PREFIX = 'BCD';

export default class UnityCommunicator {
    constructor(
        private readonly _logger: Logger,
        private readonly _tonService: TonService,
        private readonly _authApi: BackendAuthApiService,
        private readonly _obfuscate32: IObfuscate,
        private readonly _serverStatus: CheckServerService,
    ) {
        this._unityRsa = new RsaEncryption();
        this._baseAes = new AesEncryption();
        this._aesHelper = new AesEncryptionHelper(this._baseAes, this._obfuscate32);
    }

    private readonly _unityRsa: RsaEncryption;
    //private readonly _serverRsa: RsaEncryption
    private readonly _baseAes: AesEncryption;
    private readonly _aesHelper: AesEncryptionHelper;

    private _handShakeCompleted = false;

    public async isMaintenance(): Promise<string> {
        const result = await this._serverStatus.checkServerMaintenance();
        return this._aesHelper.encrypt(JSON.stringify(result));
    }

    public async handShakeFromUnity(requestData: string): Promise<string | null> {
        try {
            this._logger.log(`${TAG} handShakeFromUnity: ${requestData}`);
            const rsaPublicKey = this._obfuscate32.deobfuscate(requestData);
            this._logger.log(`publicKey: ${rsaPublicKey}`);
            this._unityRsa.importPublicKey(rsaPublicKey);
            this._baseAes.generateKey();

            const aesKey = this._baseAes.getKeyBase64();
            const encryptedAesKey = this._unityRsa.encrypt(this._obfuscate32.obfuscate(aesKey));
            const response = this._obfuscate32.obfuscate(encryptedAesKey);
            this._handShakeCompleted = true;
            return response;
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    public async payment(data: string): Promise<string> {
        if (!this._handShakeCompleted) {
            this._logger.error(`${TAG} Handshake is not completed`);

            return this._aesHelper.encrypt(JSON.stringify(false));
        }
        const decrypted = this._aesHelper.decrypt(data);
        if (!decrypted) {
            this._logger.error(`${TAG} Cannot decrypt data`);
            return this._aesHelper.encrypt(JSON.stringify(false));
        }
        const depositData = JSON.parse(decrypted) as { invoice: string, amount: number };
        let result = false;
        if (depositData.invoice.startsWith(K_TRANSFER_TON_PREFIX)) {
            result = await this._tonService.transferTon(depositData.amount, depositData.invoice);
        }
        if (depositData.invoice.startsWith(K_TRANSFER_BCOIN_PREFIX)) {
            result = await this._tonService.transferBcoin(depositData.amount, depositData.invoice);
        }
        return this._aesHelper.encrypt(JSON.stringify(result));
    }

    public async getDataTelegram() {
        if (await this._serverStatus.checkServerMaintenance()) {
            return null;
        }
        
        while (true) { 
            const account = this._authApi.getAccountTon();
            let token = account.jwt;
            const address = account.address;
            let publicKey = account.publicKey;
            
            if (!token || !address || !publicKey) {
                this._logger.log(`${TAG} Waiting for connect wallet...`);
                await sleep(1000);
                continue;
            }
            
            // If we have all required data, proceed with normal flow
            logger.log(`${TAG} jwt: ${token}`);
            logger.log(`${TAG} addr1: ${address}`);
            const jwtValidation = validateJwt(this._logger, token);
            logger.log(`${TAG} jwtValidation: ${jwtValidation} savedWalletAddress ${address}`);

            if (jwtValidation === JwtValidation.Valid) {
                logger.log(`${TAG} jwt is valid`);
                this._authApi.scheduleRefreshJwt();
            } else if (jwtValidation === JwtValidation.Expired) {
                logger.log(`${TAG} jwt is expired`);
                const newJwt = await this._authApi.refreshJwt();
                if (!newJwt) {
                    //this.logout().then();
                    return null;
                }
                this._authApi.scheduleRefreshJwt();
                token = newJwt.auth;
                publicKey = newJwt.key;

            } else if (jwtValidation === JwtValidation.Error) {
                logger.error(`${TAG} jwt is error`);
                return null;
                //this.logout().then();
            }

            const wallet = this.convertHexToBounceable(address);

            const resData: CmdDataGetJwt = {
                walletAddress: wallet,
                encryptedJwt: token,
                serverPublicKey: publicKey,
                walletHex: address
            }
            const resJson = JSON.stringify(resData);
            return this._aesHelper.encrypt(resJson);
        }
    }

    public async logout() {
        this._authApi.unScheduleRefreshJwt();
        this._authApi.clear();
        await this._tonService.logout();
        //FIXME: Chỗ này ko cần reload lại window nữa mà react sẽ gọi unity để reload về connect scene
        window.location.reload();
    }

    public async copyToClipboard(data: string) {
        const decryptedData = this._aesHelper.decrypt(data);
        if (decryptedData === null) {
            this._logger.log(`${TAG} Data to copy is null`);
            return;
        }
        copyToClipboard(decryptedData).then();
    }

    public async openUrl(url: string) {
        const decryptedUrl = this._aesHelper.decrypt(url);
        if (decryptedUrl === null) {
            this._logger.log(`${TAG} Url to open is null`);
            return;
        }
        window.open(decryptedUrl, '_blank');
    }

    public async getStartParams() {
        const urlParams = new URLSearchParams(window.location.search);
        let data = urlParams.get('code');
        if (data === null) {
            this._logger.log(`${TAG} Start params (code) is null`);
            data = "";
        }
        return this._aesHelper.encrypt(data);
    }

    public async isIOSBrowser() {
        const isIos = (/iPhone|iPad|iPod/i.test(navigator.userAgent));
        return this._aesHelper.encrypt(JSON.stringify(isIos));
    }

    public async isAndroidBrowser() {
        const isAndroid = (/Android/i.test(navigator.userAgent));
        return this._aesHelper.encrypt(JSON.stringify(isAndroid));
    }

    public async getFriendlyWalletAddress() {
        const address = authApi.getFriendlyWalletAddress();
        if (address === null) {
            this._logger.log(`${TAG} Wallet address is null`);
            return this._aesHelper.encrypt("");
        }
        return this._aesHelper.encrypt(address);
    }

    private convertHexToBounceable(hexAddress: string): string {
        // Create an Address instance from the HEX address
        const address = Address.parseRaw(hexAddress);

        return address.toString({bounceable: false, urlSafe: true, testOnly: !EnvConfig.isProduction()});
    }
}

export function validateJwt(logger: Logger, jwt: string | null): JwtValidation {
    if (!jwt) {
        logger.error(`Error: Missing jwt`);
        return JwtValidation.Error;
    }
    try {
        // check if jwt is not expired
        const decoded = decodeJwt(jwt);
        const exp = decoded.exp;
        if (!exp) {
            logger.error(`Error: Missing exp`);
            return JwtValidation.Error;
        }
        const now = Math.floor(Date.now() / 1000);
        // jwt must at least 5 minutes before expired
        if (exp - now < 300) {
            logger.error(`Error: Jwt is expired`);
            return JwtValidation.Expired;
        }
        const jwtWalletAddress = decoded.address as string;
        if (!jwtWalletAddress) {
            logger.error(`Error: Missing address`);
            return JwtValidation.NoAddress;
        }
        return JwtValidation.Valid;
    } catch (e) {
        logger.error(`Error: ${(e as Error).message}`);
        return JwtValidation.Error;
    }
}

export enum JwtValidation {
    Error, Expired, NoAddress, Valid
}

type CmdDataGetJwt = {
    walletAddress: string,
    encryptedJwt: string,
    serverPublicKey: string,
    walletHex: string,
};
