import Logger from "./Logger.ts";
import {Provider} from "@reown/appkit-adapter-ethers";
import {BrowserProvider} from 'ethers'
import {sleep} from "../utils/Time.ts";
import NotificationService from "./NotificationService.ts";
import AesEncryption from "./encrypt/AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, byteArrayToUint32} from "../utils/String.ts";
import {
    getBrowserProvider,
    setBrowserProvider,
    setProvider,
    setWalletGate
} from "./BlockChain/Module/Utils/Storage.ts";
import {forceSwapChain, getChainId, getLiveChainId, getWalletAddress, withWalletLock,} from "./WalletUtils.ts";
import {
    ChainId,
    getRpc,
    getRpcByChainId,
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
        setWalletGate({
            getAddress: () => this._walletAddress,
            run: (chainId, fn) => this.withWalletOnChain(chainId, fn),
        });

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
                    this._walletAddress = (result[0] as string).toLowerCase();
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
        const normalized = walletAddress.toLowerCase();

        if (this._stopUpdateWalletInfo) {
            this._logger.log(`${TAG} updateWalletAddress: ignored`);
            return;
        }

        this._logger.log(`${TAG} updateWalletAddress to ${normalized}`);
        this._walletAddress = normalized;
        // Update LoginModal with new wallet address
        LoginModal.setAddress(normalized);
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

    // Before the login popup: take the network the wallet is really on right now
    async syncChainFromWallet(): Promise<void> {
        if (this._stopUpdateWalletInfo || !this._etherProvider) {
            return;
        }
        const live = await getChainId(this._logger, this._etherProvider);
        if (live) {
            this.updateChainId(live.dec, undefined);
        }
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

    resumeUpdateWalletInfo() {
        this._stopUpdateWalletInfo = false;
    }

    getWalletProvider(): BrowserProvider | null {
        return this._etherProvider;
    }

    getChainId(): ChainId | null {
        return this._chainId;
    }

    setChainId(chainId: ChainId) {
        const network = getSupportedNetworkFromChainId(chainId.dec);
        if (!network) {
            this._logger.error(`${TAG} Invalid or does not support this chain ID: ${chainId}`);
            return;
        }
        this._chainId = chainId;
    }

    getCurrentWalletAddress(): string | null {
        return this._walletAddress;
    }

    allowUpdateFooterByWallet(allow: boolean) {
        this._isAllowUpdateFooter = allow;
    }

    setWalletAddress(walletAddress: string | null) {
        this._walletAddress = walletAddress?.toLowerCase() ?? null;
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
            await this._etherProvider.send('eth_requestAccounts', []);
        } catch (e) {
            this._logger.errors(`${TAG} connectWallet error:`, e);
            return;
        }

        // AppKit's switchNetwork doesn't reliably auto-add missing networks via
        // wallet_addEthereumChain. Use our own forceSwapChain (it falls back to
        // wallet_addEthereumChain on 4902).
        const networkType = this.currentNetworkType;
        const rpc = networkType ? getRpc(networkType, this._isProd) : undefined;
        if (!rpc) {
            this._switchNetworkFunc?.();
            return;
        }
        const swapped = await forceSwapChain(this._logger, this._etherProvider, rpc);
        if (!swapped) {
            this._logger.error(`${TAG} Failed to swap to ${rpc.chainName} during connect`);
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
            // A disconnect (e.g. while switching network in AppKit) must not pick the account path for the user
            if (isConnected) {
                unityCommunicator.continueConnection(true).then();
            }

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
            this._walletAddress = addressOnMetaMask?.toLowerCase() ?? null;
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

    // The wallet + network this tab logged in with (not what the wallet shows now)
    getLoginProfile(): ReadyToSign | undefined {
        const showError = this._notificationService.showError.bind(this._notificationService);
        const network = this.currentNetworkType;
        if (!this._chainId || !network) {
            this._logger.error(`${TAG} No chainId found`);
            showError("No chainId found. Please check your wallet connection.");
            return undefined;
        }
        if (!this._walletAddress) {
            this._logger.error(`${TAG} No wallet address found`);
            showError("Error happened. Please check your wallet connection.");
            return undefined;
        }
        return {
            walletAddress: this._walletAddress,
            chainId: this._chainId,
            network: network,
            isProd: this._isProd
        };
    }

    /**
     * Sau khi AppKit connect xong, ví thật có thể đang ở chain khác với chain
     * AppKit nghĩ. AppKit's switchNetwork không reliably auto-add network thiếu,
     * nên ta tự reconcile bằng forceSwapChain (có fallback wallet_addEthereumChain).
     */
    async ensureCorrectChain(): Promise<void> {
        // Logged in: the wallet network may belong to another tab, never pull it back
        if (this._stopUpdateWalletInfo) return;
        if (!this._etherProvider) return;
        const networkType = this.currentNetworkType;
        if (!networkType) return;
        const rpc = getRpc(networkType, this._isProd);
        if (!rpc) return;

        const actualChain = await getChainId(this._logger, this._etherProvider);
        if (actualChain && actualChain.dec === rpc.chainId) return;

        await forceSwapChain(this._logger, this._etherProvider, rpc);
    }

    /**
     * Runs fn with the wallet on chainId, switching it only when needed. Other
     * tabs may play another network on the same wallet, so the switch is done
     * right before signing and held under a cross-tab lock until fn is done.
     */
    async withWalletOnChain<T>(chainId: number, fn: (provider: BrowserProvider) => Promise<T>): Promise<T> {
        return withWalletLock(async () => {
            const provider = await this.switchWalletTo(chainId);
            return await fn(provider);
        });
    }

    private async switchWalletTo(chainId: number): Promise<BrowserProvider> {
        const showError = this._notificationService.showError.bind(this._notificationService);
        const walletProvider = this._walletProvider;
        if (!walletProvider) {
            showError("Wallet provider is not connected. Please check your wallet connection.");
            throw new Error('Wallet provider is not set');
        }
        const rpc = getRpcByChainId(chainId);
        if (!rpc) {
            showError("Unsupported network. Please check your wallet connection.");
            throw new Error(`No RPC config for chainId ${chainId}`);
        }

        if (await getLiveChainId(new BrowserProvider(walletProvider)) !== chainId) {
            this._logger.log(`${TAG} Wallet is on another network, switching to ${rpc.chainName}`);
            const swapped = await forceSwapChain(this._logger, new BrowserProvider(walletProvider), rpc);
            if (!swapped || !await this.waitForWalletChain(walletProvider, chainId)) {
                showError(`Failed to switch to ${rpc.chainName} network.`);
                throw new Error(`Wallet did not switch to chainId ${chainId}`);
            }
        }

        // ethers v6 BrowserProvider caches its network, a fresh one can't be stale
        const provider = new BrowserProvider(walletProvider);
        this._etherProvider = provider;
        setBrowserProvider(provider);

        const loginAddress = this._walletAddress;
        if (loginAddress) {
            const accounts = await provider.send('eth_accounts', []) as string[];
            if (!(accounts ?? []).some(a => a.toLowerCase() === loginAddress)) {
                this._logger.error(`${TAG} ${loginAddress} is not connected in the wallet`);
                showError("Wallet address mismatch. Please connect to your previous wallet.");
                throw new Error('Wallet address mismatch');
            }
        }
        return provider;
    }

    // MetaMask reports the new chain asynchronously after a switch
    private async waitForWalletChain(walletProvider: Provider, chainId: number): Promise<boolean> {
        for (let i = 0; i < 15; i++) {
            if (await getLiveChainId(new BrowserProvider(walletProvider)) === chainId) {
                return true;
            }
            await sleep(300);
        }
        this._logger.error(`${TAG} Wallet chain did not settle to ${chainId}`);
        return false;
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
        // ethers v6 BrowserProvider caches its network on first use and throws
        // NETWORK_ERROR after a chain switch. Recreate so reads work again.
        if (this._walletProvider) {
            this._etherProvider = new BrowserProvider(this._walletProvider);
            setBrowserProvider(this._etherProvider);
        }
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