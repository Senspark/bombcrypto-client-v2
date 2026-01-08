import Logger from "../Logger.ts";
import RsaEncryption from "../encrypt/RsaEncryption.ts";
import AuthService from "../AuthService.ts";
import WalletService from "../WalletService.ts";
import NotificationService from "../NotificationService.ts";
import AesEncryptionHelper from "../encrypt/AesEncryptionHelper.ts";
import AesEncryption from "../encrypt/AesEncryption.ts";
import IObfuscate from "../encrypt/IObfuscate.ts";
import VersionManager from "../VersionManager.ts";

export default class UnityCommunicator {
    constructor(
        private readonly _logger: Logger,
        private readonly _authService: AuthService,
        private readonly _walletService: WalletService,
        private readonly _notification: NotificationService,
        private readonly _obfuscate32: IObfuscate,
        private readonly _versionManager: VersionManager
    ) {
        this._unityRsa = new RsaEncryption();
        this._serverRsa = new RsaEncryption();
        this._baseAes = new AesEncryption();
        this._aesHelper = new AesEncryptionHelper(this._baseAes, this._obfuscate32);
    }

    private readonly _unityRsa: RsaEncryption;
    private readonly _serverRsa: RsaEncryption
    private readonly _baseAes: AesEncryption;
    private readonly _aesHelper: AesEncryptionHelper;


    private _handShakeCompleted = false;

    async handShakeFromUnity(requestData: string): Promise<string | null> {
        try {

            this._logger.log(`handShakeFromUnity: ${requestData}`);
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

    async getJwtForUnity(): Promise<string | null> {
        try {
            this._logger.log("getJwtForUnity");
            const jwtData = await this._authService.getJwt();
            if (!jwtData || !jwtData.jwt || !jwtData.walletAddress) {
                this._logger.error("JWT is null");
                return null;
            }
            
            const isValidVersion = await this._versionManager.checkVersion(jwtData.version);
            if (!isValidVersion) {
                this._logger.error("Version is not valid");
                return null;
            }

            const serverPublicKey = await this._authService.getServerPublicKey();
            if (!serverPublicKey) {
                this._logger.error("Server public key is null");
                return null;
            }

            this._serverRsa.importPublicKey(serverPublicKey);
            const data: CmdDataGetJwt = {
                walletAddress: jwtData.walletAddress,
                encryptedJwt: jwtData.jwt,
                serverPublicKey: serverPublicKey,
            };
            const json = JSON.stringify(data);
            return this._aesHelper.encrypt(json);
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async logout(): Promise<string | null> {
        try {
            this._logger.log("Logout");
            await this._authService.logout();
            return null
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async depositSol(requestData: string): Promise<string> {
        const result = await this._depositSol(requestData);
        return this._aesHelper.encrypt(result.toString());
    }

    async depositBcoin(requestData: string): Promise<string> {
        const result = await this._depositBcoin(requestData);
        return this._aesHelper.encrypt(result.toString());
    }

    private async _depositSol(requestData: string): Promise<boolean> {
        if (!this._handShakeCompleted) {
            this._logger.error("Handshake is not completed");
            return false;
        }
        try {
            await this._walletService.isReady();
            const decrypted = this._aesHelper.decrypt(requestData);
            if (!decrypted) {
                this._logger.error("Cannot decrypt data");
                return false;
            }

            const depositData = JSON.parse(decrypted) as CmdDataDeposit;
            if (!depositData.invoiceCode || !depositData.amount) {
                this._logger.error("Invalid data");
                return false;
            }

            const txLink = await this._walletService.depositSol(depositData.invoiceCode, depositData.amount);
            if (!txLink) {
                this._logger.error("Cannot deposit BCOIN");
                return false;
            }
            this._notification.showDepositSuccess(txLink);
            return true;
        } catch (e) {
            this._logger.error((e as Error).message);
            return false;
        }
    }

    private async _depositBcoin(requestData: string): Promise<boolean> {
        if (!this._handShakeCompleted) {
            this._logger.error("Handshake is not completed");
            return false;
        }
        try {
            await this._walletService.isReady();
            const decrypted = this._aesHelper.decrypt(requestData);
            if (!decrypted) {
                this._logger.error("Cannot decrypt data");
                return false;
            }

            const depositData = JSON.parse(decrypted) as CmdDataDeposit;
            if (!depositData.invoiceCode || !depositData.amount) {
                this._logger.error("Invalid data");
                return false;
            }

            const txLink = await this._walletService.depositBcoin(depositData.invoiceCode, depositData.amount);
            if (!txLink) {
                this._logger.error("Cannot deposit SOL");
                return false;
            }
            this._notification.showDepositSuccess(txLink);
            return true;
        } catch (e) {
            this._logger.error((e as Error).message);
            return false;
        }
    }

    sendMessageToUnity(cmd: string, data: string) {

        this._logger.log(`send message to unity: ${data}`);
        const decrypted = this._aesHelper.encrypt(data);
        const message = JSON.stringify({cmd: cmd, data: decrypted});
        window.unityInstance?.SendMessage('JsProcessor', 'CallUnity', message);
    }

    /**
     * Do có 1 số event react gửi về unity truớc khi object encryption đc tạo ví dụ như reload unity
     * nên có thêm method này để gửi message không encrypt
     * @param cmd
     * @param data
     */
    sendMessageToUnityNoEncrypt(cmd: string, data: string) {
        this._logger.log(`$send message no encrypt to unity: ${data}`);
        const message = JSON.stringify({cmd: cmd, data: data});
        window.unityInstance?.SendMessage('JsProcessor', 'CallUnity', message);
    }
}

type CmdDataGetJwt = {
    walletAddress: string,
    encryptedJwt: string,
    serverPublicKey: string,
};

type CmdDataDeposit = {
    invoiceCode: string,
    amount: number,
}