import Logger from "../Logger.ts";
import RsaEncryption from "../encrypt/RsaEncryption.ts";
import {Account, HandshakeType} from "../WalletService.ts";
import AesEncryptionHelper from "../encrypt/AesEncryptionHelper.ts";
import AesEncryption from "../encrypt/AesEncryption.ts";
import IObfuscate from "../encrypt/IObfuscate.ts";
import {BlockChainConfig} from "../BlockChain/BlockChainConfig.ts";
import {
    depositServiceWithInvoice,
    customSessionStorage,
    sessionSetting,
    walletService
} from "../../hooks/GlobalServices.ts";
import unityBridge from "./UnityBridge.ts";
import {IAuthService} from "../IAuthService.ts";
import WalletService from "../WalletService.ts";
import { appKitButtonAtom } from '../../components/AppKitButtonAtom.ts';
import { getDefaultStore } from 'jotai';
import NotificationService from "../NotificationService.ts";
import { getSupportedNetworkFromChainId, getRpc } from "../RpcNetworkUtils.ts";

const TAG = '[UC]';
const K_TRANSFER_AIRDROP_PREFIX = 'DEP';

export default class UnityCommunicator {
    constructor(
        private readonly _logger: Logger,
        private readonly _authService: IAuthService,
        private readonly _walletService: WalletService,
        private readonly _obfuscate32: IObfuscate,
        private readonly _isProd: boolean,
        private readonly _notiService: NotificationService,
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

    private _pendingConnectionPromise: Promise<string | null> | null = null;
    private _pendingConnectionResolve: ((value: string | null) => void) | null = null;
    
    private _isContinue = false;

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

    /**
     * Đây là hàm luôn đc client gọi đầu tiên khi load Unity
     * Hàm này sẽ trả về các thông tin cần thiết của wallet hoặc account này cũng như có dùng wallet hay dùng account
     */
    async getFirstDataConnection(): Promise<string | null> {
        try {
            this._logger.log(`${TAG} getFirstDataConnection`);
            walletService.allowUpdateFooterByWallet(true);
            
            // Kiểm tra trong session trước để biết được user này f5 lại tab cũ hay mở 1 tab mới và có dùng wallet hay ko
            const isUseWalletValue = await customSessionStorage.get(sessionSetting.getSessionKey().isUseWallet);
            
            // value của key là false, user này f5 lại tab cũ và lần trước ko có connect wallet nên giờ sẽ show button connect app-kit
            if (isUseWalletValue === 'false') {
                this._logger.log(`${TAG} isUseWallet is false, forcing connection to false`);
                window.setUseAccount(true);

                //Nhưng phải kiểm tra xem giờ user có đang connect wallet hay ko
                const connection = this._walletService.getConnection();
                const result = connection === true;

                // Nếu bây giờ user đã có connect rồi nên sẽ phải show button connect wallet fake
                // còn nếu ko có connect wallet thì show button connect wallet của app-kit để nếu lỡ user có muốn chơi wallet
                getDefaultStore().set(appKitButtonAtom, prev => ({
                    ...prev,
                    showFakeConnect: result
                }));
                
                // Chờ user chọn chơi account hay network
               // return "false";
            }
            
            // Value là true, user này f5 lại và lần trước có dùng wallet
            else if (isUseWalletValue === 'true') {
                // Có lưu useWallet trong session rồi nên sẽ ko show nút connect fake nữa mà show cái của app-kit
                // Ko cần quan tâm trạng thái của wallet bây giờ, nó đang là cái gì thì app-kit sẽ hiên cho phù hợp
                getDefaultStore().set(appKitButtonAtom, prev => ({
                    ...prev,
                    showFakeConnect: false
                }));
                // Ko có connect ví nữa nên sẽ show nút login account cho chọn login bằng account
                const connection = this._walletService.getConnection();
                const result = connection ? "true" : "false";
                this._logger.log(`${TAG} isUseWallet is true, returning connection: ${result} from app-kit`);
                window.setUseAccount(!(connection === true));

                // Chỉ khi còn đang connect ví mới cho vô luôn
                if(connection) {
                    window.showFooterContent?.(true);
                    // tắt video đi
                    if (typeof window.enableBgVideo === 'function') {
                        window.enableBgVideo(false);
                    }
                    return result;
                }
                
                // Đã disconnect ví rồi, phải chờ user chọn lại ví hay account
            }

            // Ko có key này trong session, đây là user mở trên 1 tab mới, sẽ luôn hiện 2 nút connect account và wallet cho user
            else{
                // Kiểm tra thử xem giờ user có connect wallet chưa
                const connection = this._walletService.getConnection();

                // Ko có connect wallet, show button app-kit để user có thể chọn connect
                this._logger.log(`${TAG} New tab, connection is ${connection}, wait for user choose connect`);
                window.setUseAccount(true);

                const result = connection === true;
                // có connect wallet sẵn thì show nút fake để ko thấy wallet data, còn chưa connect thì show nút connect của app-kit
                getDefaultStore().set(appKitButtonAtom, prev => ({
                    ...prev,
                    showFakeConnect: result
                }));
            }

            // Show the footer content, hiding the loading UI
            window.showFooterContent?.(true); 
            
            // đợi cho user chọn connect wallet hay connect account
            if (!this._pendingConnectionPromise) {
                this._pendingConnectionPromise = new Promise<string | null>((resolve) => {
                    this._pendingConnectionResolve = resolve;
                });
            }
            
            const result = await this._pendingConnectionPromise;
            // tắt video đi
            if (typeof window.enableBgVideo === 'function') {
                window.enableBgVideo(false);
            }
            return result;
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
    }
    
    allowContinueConnection(allow: boolean): void {
        this._isContinue = !allow;
    }
    
    // Method to be called when user clicks Connect Wallet button
    async continueConnection(useWallet: boolean = false): Promise<void> {
        try {
            if(this._isContinue)
            {
                this._logger.log(`${TAG} already continueConnection, return`);
                return;
            }
            this._logger.log(`${TAG} continueConnection called`);
            
            // Set isUseWallet to true in sessionStorage
            await customSessionStorage.set(sessionSetting.getSessionKey().isUseWallet, useWallet ? 'true' : 'false');
            
            // Set showFakeConnect to false
            getDefaultStore().set(appKitButtonAtom, prev => ({ 
                ...prev, 
                showFakeConnect: !useWallet 
            }));
            
            // Resolve the pending promise if it exists
            if (this._pendingConnectionResolve) {
                const result = useWallet ? "true" : "false";
                this._logger.log(`${TAG} Resolving pending connection with: ${result}`);
                this._pendingConnectionResolve(result);
                this._pendingConnectionPromise = null;
                this._pendingConnectionResolve = null;
            }
            this._isContinue = true;

        } catch (e) {
            this._logger.error(`${TAG} Error in continueConnection: ${e}`);
        }
    }
    
    //React sẽ tự bật tắt video, ko cần unity gọi nữa
    async enableVideoThumbnail(data: string): Promise<string | null> {
        const state = data.toLowerCase() === 'true';
        this._logger.log(`${TAG} enableVideoThumbnail ${state}`);
        // window.enableBgVideo(state);
        return null;
    }

    async getLoginData(): Promise<string | null> {
        try {
            let accountData = await this._authService.getAccountData();
            while (accountData === null) {
                this._logger.log(`!@#DevHoang accountData is null, retrying...`);
                await new Promise(res => setTimeout(res, 1000)); // wait 1000ms
                accountData = await this._authService.getAccountData();
            }
            while (accountData.network === null) {
                this._logger.log(`!@#DevHoang accountData network is null, waiting...`);
                await new Promise(res => setTimeout(res, 1000)); // wait 1000ms
            }
            this._logger.log(`!@#DevHoang accountData is not null, returning... ${JSON.stringify(accountData)}`);
            return JSON.stringify(accountData);
        } catch (e) {
            this._logger.error((e as Error).message);
            return null;
        }
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


            // User chọn login bằng wallet rồi, cần set cái này vào session để có f5 thì cũng biết user này muốn chơi wallet
            await customSessionStorage.set(sessionSetting.getSessionKey().isUseWallet, 'true');


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

            // Ko cần disconnect ví khi login account nữa, khi đã login rồi thì ví có update gì thì cũng mặc kệ
            //Disconnect ví nếu đang có kết nối vì user đang login account senspark
            // const isLogout = this._walletService.getWalletProvider() == null;
            // if (!isLogout) {
            //     await this.logoutNoReload();
            // }

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
            //DevHoang: Check khi login account xong
            window.setUseAccount(false);

            const accountData = await this._authService.getAccountData();
            if (!accountData) {
                this._logger.error(`${TAG} Account data is null`);
                return null;
            }

            if (dataAccount.isUserFi && accountData.network) {
                const network = getRpc(accountData.network, this._isProd);
                getDefaultStore().set(appKitButtonAtom, { showCustomUI: true, network: network?.chainName, address: account.userName, showFakeConnect: false });
            } else {
                getDefaultStore().set(appKitButtonAtom, { showCustomUI: true, network: "", address: account.userName, showFakeConnect: false });
            }

            // user đã chọn login account rồi, cần lưu lại để có f5 cũng biết là user này muốn chơi account
            await customSessionStorage.set(sessionSetting.getSessionKey().isUseWallet, 'false');


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
        const showError = this._notiService.showError.bind(this._notiService);

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
            const chainId = this._walletService.getChainId();
            const userChangeChainId = this._walletService.getUserChangeChainId();
            const walletAddress = this._walletService.getCurrentWalletAddress();
            const userChangeWalletAddress = this._walletService.getUserChangeWalletAddress();

            //DevHoang: Nếu user đã thay đổi walletAddress
            if (walletAddress && userChangeWalletAddress && walletAddress !== userChangeWalletAddress) {
                this._logger.log(`${TAG} User changed wallet address, switching wallet...`);
                return null;
            }
            
            //DevHoang: Nếu user đã thay đổi chainId thì sẽ gọi forceSwapChain để về lại chainId ban đầu
            if (userChangeChainId && chainId && userChangeChainId.dec !== chainId.dec) {
                this._logger.log(`${TAG} User changed chain ID, switching chain...`);
                const network = getSupportedNetworkFromChainId(chainId.dec, this._isProd);
                showError(`Network Changed! Changing back to ${network}`);
                const success = await this._walletService.forceSwapChain();
                if (!success) {
                    this._logger.error(`${TAG} Failed to switch chain`);
                    return null;
                }
                this._walletService.setUserChangeChainId(chainId);
            }
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
            const currentChainId = this._walletService.getChainId()?.dec;
            const depositChainId = Number(depositData.chainId);
            if (depositChainId !== currentChainId) {
                //DevHoang: Force to change metamask
                const result = await walletService.forceSwapChain()
                if (!result) {
                    this._logger.error(`${TAG} Failed to switch chain`);
                    return this._aesHelper.encrypt(JSON.stringify(false));
                }
            }
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
