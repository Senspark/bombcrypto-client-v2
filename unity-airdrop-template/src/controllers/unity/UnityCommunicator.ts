import Logger from "../Logger.ts";
import RsaEncryption from "../encrypt/RsaEncryption.ts";
import {Account, HandshakeType} from "../WalletService.ts";
import AesEncryptionHelper from "../encrypt/AesEncryptionHelper.ts";
import AesEncryption from "../encrypt/AesEncryption.ts";
import IObfuscate from "../encrypt/IObfuscate.ts";
import {BlockChainConfig} from "../BlockChain/BlockChainConfig.ts";
import {depositServiceWithInvoice} from "../../hooks/GlobalServices.ts";
import unityBridge from "./UnityBridge.ts";
import {IAuthService} from "../IAuthService.ts";
import RonBaseWalletService from "../RonBaseWalletService.ts";

const TAG = '[UC]';
const K_TRANSFER_AIRDROP_PREFIX = 'DEP';

export default class UnityCommunicator {
    constructor(
        private readonly _logger: Logger,
        private readonly _authService: IAuthService,
        private readonly _walletService: RonBaseWalletService,
        private readonly _obfuscate32: IObfuscate,
        private readonly _isProd: boolean
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

    private _blockChainConfig: BlockChainConfig | null = null;
    private _handShakeCompleted = false;

    async handShakeFromUnity(requestData: string): Promise<string | null> {
        try {
            this._logger.log(`${TAG} handShakeFromUnity: ${requestData}`);
            const rsaPublicKey = this._obfuscate32.deobfuscate(requestData);
            this._logger.log(`${TAG} publicKey: ${rsaPublicKey}`);
            this._unityRsa.importPublicKey(rsaPublicKey);
            this._baseAes.generateKey();

            const aesKey = this._baseAes.getKeyBase64();
            const encryptedAesKey = this._unityRsa.encrypt(this._obfuscate32.obfuscate(aesKey));
            const response = this._obfuscate32.obfuscate(encryptedAesKey);
            this._handShakeCompleted = true;
            this._logger.log(`${TAG} handShakeFromUnity completed: ${this._handShakeCompleted}`);
            window.onReactSendLog?.(); // Allow to send log to Unity
            return response;
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async getConnection(): Promise<string | null> {
        try {
            this._logger.log(`${TAG} getConnection`);
            const connection = this._walletService.getConnection();
            if (!connection) {
                return "false";
            }
            return connection ? "true" : "false";
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    // React tự handle show video, ko phụ thuộc unity nữa
    async enableVideoThumbnail(data: string): Promise<string | null> {
        // const state = data.toLowerCase() === 'true';
        // this._logger.log(`${TAG} enableVideoThumbnail ${state}`);
        // window.enableBgVideo(state);
        this._logger.log(`${TAG} enableVideoThumbnail ${data}`);
        return null;
    }

    // React tự handle show video, ko phụ thuộc unity nữa
    // Đây là template của RON BASE nên sẽ ko có nhập userName password, bỏ qua bước này
    // Sau này có merge với BSC/ POL thì fix lại chỗ này
    async getLoginData(): Promise<string | null> {
        try {
            // window.setLoginState('enable');
            // return await this.getInputFromPopup("showLogin");
            // FIXME : return tạm để unity parse ko lỗi, sẽ sửa lại sau
            return JSON.stringify({
                userName: "tester",
                password: "tester"
            });
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async getNetworkData(): Promise<string | null> {
        try {
            window.setOpenConnectNetwork(true);
            return await this.getInputFromPopup("showNetwork");
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    private async getInputFromPopup(eventName: string): Promise<string> {
        return new Promise((resolve) => {
            this._logger.log(`${TAG} Waiting for ${eventName} input from UI...`);

            const resolver = (data: string) => {
                this._logger.log(`${TAG} Received ${eventName} input: ${data}`);
                resolve(data);
            };

            const event = new CustomEvent(eventName, {
                detail: {
                    onSubmit: resolver,
                },
            });

            window.dispatchEvent(event);
        });
    }

    async getJwtForWallet(handshakeType: string): Promise<string | null> {
        try {
            this._logger.log(`${TAG} getJwtForUnity`);

            let type: HandshakeType = "Login";
            const decrypted = this._aesHelper.decrypt(handshakeType);
            if (decrypted == null) {
                this._logger.error(`${TAG} Cannot decrypt account data or no data`);
            } else {
                type = decrypted as HandshakeType || "Login";
            }

            const jwtData = await this._authService.getJwt(type);
            if (!jwtData || !jwtData.jwt || !jwtData.walletAddress) {
                this._logger.error(`${TAG} JWT is null`);
                return null;
            }

            const serverPublicKey = await this._authService.getServerPublicKey();
            if (!serverPublicKey) {
                this._logger.error(`${TAG} Server public key is null`);
                return null;
            }

            this._serverRsa.importPublicKey(serverPublicKey);

            const chainId = this._walletService.getChainId();
            const data: CmdDataGetJwt = {
                walletAddress: jwtData.walletAddress,
                encryptedJwt: jwtData.jwt,
                serverPublicKey: serverPublicKey,
                chainId: chainId?.dec.toString() || ''
            };
            const json = JSON.stringify(data);
            return this._aesHelper.encrypt(json);
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async getJwtForAccount(data: string): Promise<string | null> {
        try {
            this._logger.log(`${TAG} getJwtForAccount`);
            const account = this.parseAccountData(data);
            if (account == null) {
                this._logger.error(`${TAG} Cannot parse account data`);
                return null;
            }

            const jwtData = await this._authService.getJwtForAccount(account);
            if (!jwtData || !jwtData.jwt) {
                this._logger.error(`${TAG} JWT is null`);
                return null;
            }

            //Disconnect ví nếu đang có kết nối vì user đang login account senspark
            const isLogout = this._walletService.getWalletProvider() == null;
            if (!isLogout) {
                await this.logoutNoReload();
            }

            const serverPublicKey = await this._authService.getServerPublicKey();
            if (!serverPublicKey) {
                this._logger.error(`${TAG} Server public key is null`);
                return null;
            }

            this._serverRsa.importPublicKey(serverPublicKey);

            const dataAccount: CmdDataGetJwtForAccount = {
                encryptedJwt: jwtData.jwt,
                serverPublicKey: serverPublicKey,
                isUserFi: jwtData.isUserFi
            };
            const json = JSON.stringify(dataAccount);

            window.setConnectWallet(false);
            window.setConnectUser(true);
            window.setUserName(account.userName);

            return this._aesHelper.encrypt(json);
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    private parseAccountData(data: string): Account | null {
        const decrypted = this._aesHelper.decrypt(data);
        if (decrypted == null) {
            this._logger.error(`${TAG} Cannot decrypt account data`);
            return null;
        }
        return JSON.parse(decrypted) as Account;
    }

    async initBlockChain(): Promise<string | null> {
        const chainId = this._walletService.getChainId();
        if (!chainId) {
            this._logger.error(`${TAG} ChainId is null, can't init blockchain`);
            return null;
        }
        this._blockChainConfig = new BlockChainConfig(chainId.dec.toString(), this._isProd, this._logger, this._aesHelper);
        return null;
    }

    async callBlockChainMethod(data: string): Promise<string | null> {
        if (!this._blockChainConfig) {
            this._logger.error(`${TAG} Blockchain config is not initialized`);
            return null;
        }
        try {

            const decrypted = this._aesHelper.decrypt(data);
            if (decrypted == null) {
                this._logger.error(`${TAG} Cannot decrypt blockchain data`);
                return null;
            }
            const {name, param} = JSON.parse(decrypted) as BlockChainData;
            return await this._blockChainConfig.callAction(name, param);
        } catch (e) {
            this._logger.error(`${TAG} Call blockchain method error: ${e}`);
            return null;
        }
    }

    async changeNickName(data: string): Promise<string | null> {
        try {
            const decrypted = this._aesHelper.decrypt(data);
            if (decrypted == null) {
                this._logger.error(`${TAG} Cannot decrypt account data`);
                return this._aesHelper.encrypt(JSON.stringify(false));
            }

            const {userName, newNickName} = JSON.parse(decrypted) as { userName: string, newNickName: string };

            const result = await this._authService.changeNickName(userName, newNickName);
            return this._aesHelper.encrypt(JSON.stringify(result));
        } catch (e) {
            this._logger.error(`${TAG} Create guest account fail: ${e}`);
            return null;
        }
    }

    async logout(): Promise<string | null> {
        try {
            this._logger.log("Logout");
            // Unity gọi logout thì unity đã tự reload rồi, ko cần react gọi lại unity reload
            unityBridge.skipReloadClient();
            await this._authService.logout();
            return null
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async logoutNoReload(): Promise<string | null> {
        try {
            this._logger.log("Logout no reload");
            unityBridge.skipReloadClient();
            await this._authService.logout();
            return null
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }

    async payment(data: string): Promise<string> {
        if (!this._handShakeCompleted) {
            this._logger.error(`${TAG} Handshake is not completed`);

            return this._aesHelper.encrypt(JSON.stringify(false));
        }
        const decrypted = this._aesHelper.decrypt(data);
        if (!decrypted) {
            this._logger.error(`${TAG} Cannot decrypt data`);
            return this._aesHelper.encrypt(JSON.stringify(false));
        }
        const depositData = JSON.parse(decrypted) as { invoice: string, amount: string, chainId: string };
        let result = false;
        if (depositData.invoice.startsWith(K_TRANSFER_AIRDROP_PREFIX) && this._walletService.currentNetworkType) {
            result = await depositServiceWithInvoice.deposit(depositData.amount, depositData.invoice);
        }
        return this._aesHelper.encrypt(JSON.stringify(result));
    }

    sendMessageToUnity(cmd: string, data: string) {
        this._logger.log(`${TAG} send message to unity: ${data}`);
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
        // this._logger.log(`${TAG} send message no encrypt to unity: ${data}`);
        const message = JSON.stringify({cmd: cmd, data: data});
        window.unityInstance?.SendMessage('JsProcessor', 'CallUnity', message);
    }
}

type CmdDataGetJwt = {
    walletAddress: string,
    encryptedJwt: string,
    serverPublicKey: string,
    chainId: string,
};

type CmdDataGetJwtForAccount = {
    encryptedJwt: string,
    serverPublicKey: string,
    isUserFi: boolean,
};


type BlockChainData = {
    name: string,
    param: string,
}
