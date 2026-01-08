import Logger from "./Logger.ts";
import {Provider} from "@reown/appkit-adapter-ethers";
import {BrowserProvider} from 'ethers'
import {sleep} from "../utils/Time.ts";
import NotificationService from "./NotificationService.ts";
import AesEncryption from "./encrypt/AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, byteArrayToUint32} from "../utils/String.ts";
import {getBrowserProvider, setBrowserProvider, setProvider} from "./BlockChain/Module/Utils/Storage.ts";
import {forceSwapChain, getChainId, getWalletAddress,} from "./WalletUtils.ts";
import {
    ChainId,
    getRpc,
    getSupportedChainId,
    getSupportedNetworkFromChainId,
    SupportedNetwork
} from "./RpcNetworkUtils.ts";
import LoginModal from '../components/LoginModal';
import { appKitButtonAtom } from '../components/AppKitButtonAtom';
import { getDefaultStore } from 'jotai';
import unityBridge from "./unity/UnityBridge.ts";
import {authService, customSessionStorage, sessionSetting, unityCommunicator} from "../hooks/GlobalServices.ts";

const TAG = '[WS]';

export default class WalletService {
    constructor(
        private readonly _isProd: boolean,
        signerSecret: () => string,
        private readonly _signPadding: () => string,
        private readonly _logger: Logger,
        private readonly _notificationService: NotificationService
    ) {
        _logger.log(`${TAG} new WalletService created`);
        this._encryptor = new AesEncryption();
        this._encryptor.importKey(signerSecret());

        setTimeout(() => {
            // this._allowReloadClient = true;
            this._logger.log(`${TAG} _allowReloadClient set to true`);
        }, 2000);
    }

    
    public get currentNetworkType(): SupportedNetwork | undefined {
        if (!this._chainId) {
            return undefined;
        }
        
        return getSupportedNetworkFromChainId(this._chainId.dec, this._isProd);
    }

    public currentNetworkTypeFromChainId(chainId: ChainId): SupportedNetwork | undefined {

        return getSupportedNetworkFromChainId(chainId.dec, this._isProd);
    }

    private readonly _encryptor: AesEncryption;

    private _walletAddress: string | null = null;
    private _chainId: ChainId | null = null;
    private _switchNetworkFunc: (() => void) | null = null;
    private _walletProvider: Provider | null = null;
    private _etherProvider: BrowserProvider | null = null;
    private _isConnected: boolean | null = null;
    private _stopUpdateWalletInfo = false;
    private _userChangeChainId: ChainId | null = null;
    private _userChangeWalletAddress: string | null = null;
    // private _allowReloadClient: boolean = false;
    private _isAllowUpdateFooter: boolean = false;
    
    logOut() {
        this._stopUpdateWalletInfo = false;
        // Set null tạm thời ở đây, sau đó sẽ tự update lại từ app-kit
        this._walletAddress = null;
        this._chainId = null;
        // Reset AppKitButton to default UI using custom UI
        getDefaultStore().set(appKitButtonAtom, { showCustomUI: false});
        window.showFooterContent(false);
        // this._isConnected = false;
        // never disconnect app-kit

        //DevHoang: Check khi user logout
        // window.setUseAccount(!this._isConnected);
        customSessionStorage.remove(sessionSetting.getSessionKey().isUseWallet)
    }

    async tryGetProfileFromAppkit(){
        const timeoutMs = 3000;
        const timeoutPromise = new Promise((_, reject) => setTimeout(() => reject(new Error('Timeout after 3 seconds')), timeoutMs));
        try {
            const walletAddressPromise = this.getWalletAddress();
            const chainIdPromise = this.getCurrentNetworkFromMetaMask();
            const result = await Promise.race([
                Promise.all([walletAddressPromise, chainIdPromise]),
                timeoutPromise
            ]);
            if (Array.isArray(result) && result.length === 2) {
                if(this._walletAddress == null) {
                    this._walletAddress = result[0];
                }                
                if(this._chainId == null){
                    this._chainId = result[1];
                }
            }
        } catch (e) {
            this._logger.log("Error when try to get profile from app-kit: " + e);
            // Ignore
        }
    }

    userChangeNetwork() {
        this._chainId = null;
    }

    updateWalletAddress(walletAddress: string | undefined) {
        if (!walletAddress) {
            return;
        }
        this._userChangeWalletAddress = walletAddress;

        if (this._stopUpdateWalletInfo) {
            this._logger.log(`${TAG} updateWalletAddress: ignored`);
            return;
        }

        this._logger.log(`${TAG} updateWalletAddress to ${walletAddress}`);
        this._walletAddress = walletAddress!;
        // Update LoginModal with new wallet address
        LoginModal.setAddress(walletAddress);
    }

    updateWalletProvider(walletProvider: Provider) {
        if (this._stopUpdateWalletInfo) {
            this._logger.log(`${TAG} updateWalletProvider: ignored`);
            return;
        }

        if (!walletProvider || this._walletProvider) {
            return;
        }

        this._logger.log(`${TAG} updateWalletProvider`);

        setProvider(walletProvider)
        this._walletProvider = walletProvider;

        this._etherProvider = new BrowserProvider(this._walletProvider);
        setBrowserProvider(this._etherProvider);

        this._walletProvider.on('accountsChanged', this.onUserSelectOtherWallet.bind(this));
        this._walletProvider.on('chainChanged', this.onUserSelectOtherChain.bind(this));
    }

    async getCurrentNetworkFromMetaMask(): Promise<ChainId | null> {
        const provider = getBrowserProvider()
        return getChainId(this._logger, provider)

    }

    updateChainId(chainId: string | number | undefined, switchNetworkFunc: (() => void) | undefined) {
        if (switchNetworkFunc) {
            this._switchNetworkFunc = switchNetworkFunc;
        }
        try {
            if (this._stopUpdateWalletInfo) {
                this._logger.log(`${TAG} updateChainId: ignored`);
                return;
            }
            if (!chainId) {
                this._logger.error(`${TAG} Chain ID is not provided`);
                return;
            }
            const parseChainId = (num: number) => {
                if (isNaN(num)) {
                    this._logger.error(`${TAG} Invalid chain ID: ${num}`);
                    return;
                }
                const supportedNetwork = getSupportedNetworkFromChainId(num, this._isProd);
                if (!supportedNetwork) {
                    this._logger.error(`${TAG} Unsupported chain ID: ${num}`);
                    this._notificationService.showError("Unsupported network. Please check your wallet connection.");
                    return;
                }
                this._logger.log(`${TAG} updateChainId to ${chainId} (${supportedNetwork})`);
                this._chainId = getSupportedChainId(supportedNetwork, this._isProd)!;
                // Update LoginModal with new network
                const network = getRpc(this.currentNetworkType!, this._isProd);
                LoginModal.setNetwork(network?.chainName);
            };
            if (typeof chainId === 'number') {
                if (this._chainId && chainId === this._chainId.dec) {
                    return;
                }
                parseChainId(Number(chainId));
            }
            if (typeof chainId === 'string') {
                if (this._chainId && chainId === this._chainId.hex) {
                    return;
                }
                const num = parseInt(chainId, 16);
                parseChainId(num);
            }
        } catch (e) {
            this._logger.errors(`${TAG} updateChainId error:`, e);
        }
    }

    stopUpdateWalletInfo() {
        this._stopUpdateWalletInfo = true;
    }

    getWalletProvider(): BrowserProvider | null {
        return this._etherProvider;
    }

    getChainId(): ChainId | null {
        return this._chainId;
    }

    getUserChangeChainId(): ChainId | null {
        return this._userChangeChainId;
    }

    setChainId(chainId: ChainId) {
        const network = getSupportedNetworkFromChainId(chainId.dec);
        if (!network) {
            this._logger.error(`${TAG} Invalid or does not support this chain ID: ${chainId}`);
            return;
        }
        this._chainId = chainId;
    }

    setUserChangeChainId(chainId: ChainId) {
        this._userChangeChainId = chainId;
    }

    getCurrentWalletAddress(): string | null {
        return this._walletAddress;
    }

    getUserChangeWalletAddress(): string | null {
        return this._userChangeWalletAddress;
    }
    
    allowUpdateFooterByWallet(allow: boolean) {
        this._isAllowUpdateFooter = allow;
    }

    setWalletAddress(walletAddress: string | null) {
        this._walletAddress = walletAddress;
    }

    setUserChangeWalletAddress(walletAddress: string | null) {
        this._userChangeWalletAddress = walletAddress;
    }

    setChain(networkType: SupportedNetwork) {
        this._logger.log(`${TAG} setChain to ${networkType}`);
        const chainId = getSupportedChainId(networkType, this._isProd);
        if (!chainId) {
            this._logger.error(`${TAG} Invalid or does not support this network type: ${networkType}`);
            return;
        }
        this.setChainId(chainId);
    }

    getConnection(): boolean | null {
        return this._isConnected;
    }

    /**
     * Kết nối đến Wallet đang được select trong MetaMask
     */
    async connectWallet(): Promise<void> {
        if (!this._etherProvider) {
            this._logger.error(`${TAG} connectWallet: Ether provider is not set`);
            return;
        }
        try {
            // await this._etherProvider.send('wallet_requestPermissions', [{eth_accounts: {}}]);
            // await this._etherProvider.send('wallet_revokePermissions',  [{ eth_accounts: {} }]);
            await this._etherProvider.send('eth_requestAccounts', []);
            this._switchNetworkFunc?.();
            // request network
        } catch (e) {
            this._logger.errors(`${TAG} connectWallet error:`, e);
        }
    }

    updateWalletConnection(isConnected: boolean) {
        if (isConnected === this._isConnected) return;

        if (this._stopUpdateWalletInfo) {
            this._logger.log(`${TAG} updateWalletConnection: ignored`);
            return;
        }

        this._isConnected = isConnected;
        this._logger.log(`${TAG} updateWalletConnection ${isConnected}`);
        
        //DevHoang: Check khi load page
        if (this._isAllowUpdateFooter) {
            window.setUseAccount(!isConnected);

            customSessionStorage.set(sessionSetting.getSessionKey().isUseWallet, isConnected ? 'true' : 'false')
            unityCommunicator.continueConnection(isConnected).then();

        } else {
            this._logger.log(`${TAG} Not allow update footer yet, skip`);
        }

        //DevHoang: Reload client
        // if (this._allowReloadClient) {
        //     unityBridge.skipReloadClient(false);
        //     unityBridge.reloadClient();
        // }
    }

    async getWalletAddress(): Promise<string> {
        while (this._walletAddress == null) {
            const addressOnMetaMask = await getWalletAddress(this._logger, getBrowserProvider());
            if (!addressOnMetaMask) {
                this._logger.log(`Waiting for update wallet address`);
                await sleep(1000);
            }
            this._walletAddress = addressOnMetaMask;
        }
        return this._walletAddress;
    }

    async sign(nonce: string): Promise<string | null> {
        try {
            if (!nonce || nonce.length == 0) {
                this._logger.error(`Nonce is empty`);
                return null;
            }
            if (!this._etherProvider) {
                this._logger.error(`Ether provider is not set`);
                return null;
            }
            const signer = await this._etherProvider.getSigner()
            if (!signer) {
                this._logger.error(`Signer is not set`);
                return null;
            }
            const message = await this.generateStringToSign(nonce);
            //return Buffer.from(signedMessage).toString('base64');
            return await signer.signMessage(message);
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    async isReadyToSign(): Promise<ReadyToSign | undefined> {
        const isProd = this._isProd;
        const provider = this._etherProvider;
        const logger = this._logger;
        const showError = this._notificationService.showError.bind(this._notificationService);

        try {
            if (!this._walletProvider) {
                this._logger.error(`${TAG} Wallet provider is not set`);
                return undefined;
            }
            if (!provider) {
                logger.error(`${TAG} Ether provider is not set`);
                showError("Wallet provider is not connected. Please check your wallet connection.");
                return undefined;
            }

            const walletAddress = await getWalletAddress(logger, provider);
            if (!walletAddress) {
                logger.error(`${TAG} Failed to get wallet address from provider`);
                return undefined;
            }

            // Kiểm tra chainId có đúng là chain đã dùng để đăng nhập không?

            const walletNetwork = await getChainId(logger, provider);
            if (!walletNetwork) {
                logger.error(`${TAG} Failed to get current selected network`);
                showError("Failed to get current network. Please check your wallet connection.");
                return undefined;
            }

            // Kiểm tra network mà user đã đăng nhập rồi so với network hiện tại mà ví đang kết nối
            const loggedInNetwork = this.getChainId();
            if (!loggedInNetwork) {
                logger.error(`${TAG} No chainId found`);
                showError("No chainId found. Please check your wallet connection.");
                return undefined;
            }

            const network = getSupportedNetworkFromChainId(walletNetwork.dec, isProd);
            if (!network || (network !== 'ronin' && network !== 'base' && network !== 'vic')) {
                logger.error(`${TAG} Unsupported network: ${network}. Only 'Ronin', 'Base' and 'Vic' are supported.`);

                const swapped = await forceSwapChain(logger, provider, loggedInNetwork);
                if (!swapped) {
                    showError(`Failed to switch to Ronin or Base network.`);
                    return undefined;
                }
            } else {
                if (loggedInNetwork.dec != walletNetwork.dec) {
                    logger.error(`${TAG} Network mismatch: expected ${loggedInNetwork.dec}, got ${walletNetwork.dec}`);
                    const swapped = await forceSwapChain(logger, provider, loggedInNetwork);
                    if (!swapped) {
                        showError(`Failed to switch to previous network.`);
                        return undefined;
                    }
                }
            }

            // Kiểm tra wallet có đúng là wallet đã dùng để đăng nhập không?
            const senderAddress = await getWalletAddress(logger, provider);
            if (!senderAddress) {
                showError("Error happened. Please check your wallet connection.");
                return undefined;
            }
            if (senderAddress.toLowerCase() !== walletAddress.toLowerCase()) {
                logger.error(`${TAG} Wallet address mismatch: expected ${walletAddress}, got ${senderAddress}`);
                showError("Wallet address mismatch. Please connect to your previous wallet.");
                return undefined;
            }

            return {
                walletAddress: walletAddress,
                chainId: walletNetwork,
                network: this.currentNetworkType!,
                isProd: isProd
            };
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return undefined;
        }
    }

    async forceSwapChain() {
        const provider = this._etherProvider;
        const logger = this._logger;
        const showError = this._notificationService.showError.bind(this._notificationService);
        try {
            if (!provider) {
                logger.error(`${TAG} Ether provider is not set`);
                showError("Wallet provider is not connected. Please check your wallet connection.");
                return false;
            }

            const chosenChainId = this.getChainId();
            if (!chosenChainId) {
                logger.error(`${TAG} No chainId found`);
                showError("No chainId found. Please check your wallet connection.");
                return false;
            }
            const swapped = await forceSwapChain(logger, provider, chosenChainId);
            if (!swapped) {
                showError(`Failed to switch to ${chosenChainId} network.`);
                return false;
            }
            return true;

        } catch (e) {
            logger.error(`${TAG} Error in forceSwapChain: ${(e as Error).message}`);
            showError("Failed to swap chain. Please try again.");
            return false;
        }
    }

    getBlockExplorerUrl(txHash: string): string {
        const network = this.currentNetworkType;
        if (network) {
            const rpc = getRpc(network, this._isProd);
            if (rpc) {
                return `${rpc.blockExplorerUrl}/tx/${txHash}`;
            }
        }
        return '';
    }

    /**
     * Khi user chọn 1 ví khác trong MetaMask
     * @private
     */
    private async onUserSelectOtherWallet(accounts: string[]) {
        console.log(`${TAG} onUserSelectOtherWallet`, accounts);

        if (this._stopUpdateWalletInfo) {
            this._logger.log(`${TAG} onUserSelectOtherWallet: ignored`);
            return;
        }
        
        if (accounts && accounts.length > 0) {
            authService.clearSavedWallet();
            await authService.logout();
            this.updateWalletAddress(accounts[0]);
            unityBridge.skipReloadClient(false);
            unityBridge.reloadClient();

            // Reset and reinitialize provider
            if (window.ethereum) {
                this._walletProvider = window.ethereum as unknown as Provider;
                this._etherProvider = new BrowserProvider(this._walletProvider);
                setBrowserProvider(this._etherProvider);
            } else {
                this._logger.error(`${TAG} window.ethereum is undefined`);
                this._walletProvider = null;
                this._etherProvider = null;
            }
        }
    }

    private onUserSelectOtherChain(chainIdHex: string) {
        this._logger.log(`${TAG} onUserSelectOtherChain ${chainIdHex}`);
        this.updateChainId(chainIdHex, undefined);
    }

    private async generateStringToSign(nonceString: string): Promise<string> {
        const bytes = base64ToByteArray(nonceString);
        // first 4 bytes is nonce number
        // the rest (16 bytes) is iv
        const nonce = byteArrayToUint32(bytes.slice(0, 4));
        const iv = byteArrayToBase64(bytes.slice(4));

        const display = "Your login code:";
        const message = `${this._signPadding()}${nonce}`;
        const encryptedNonce = this._encryptor.encrypt(message, iv);
        return `${display} ${encryptedNonce}`;
    }
}

export type Account = {
    userName: string;
    password: string;
    network: SupportedNetwork | undefined | null;
}

export type HandshakeType = "Login" | "Reconnect"

export type ReadyToSign = {
    walletAddress: string;
    chainId: ChainId;
    network: SupportedNetwork;
    isProd: boolean;
}