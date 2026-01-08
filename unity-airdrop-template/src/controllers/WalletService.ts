import Logger from "./Logger.ts";
import {Provider} from "@reown/appkit-adapter-ethers";
import {BrowserProvider, ethers} from 'ethers'
import {sleep} from "../utils/Time.ts";
import NotificationService from "./NotificationService.ts";
import AesEncryption from "./encrypt/AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, byteArrayToUint32} from "../utils/String.ts";
import unityBridge from "./unity/UnityBridge.ts";
import {setBrowserProvider, setProvider} from "./BlockChain/Module/Utils/Storage.ts";
import {addNetwork, forceSwapChain, getBalance, getChainId, getWalletAddress, sendTransaction,} from "./WalletUtils.ts";
import {
    ChainId,
    getRpc,
    getSupportedChainId,
    getSupportedNetworkFromChainId,
    SupportedNetwork
} from "./RpcNetworkUtils.ts";

const TAG = '[WS]';

export default class WalletService {
    constructor(
        isProd: boolean,
        signerSecret: () => string,
        private readonly _signPadding: () => string,
        private readonly _logger: Logger,
        private readonly _notificationService: NotificationService
    ) {
        _logger.log(`${TAG} new WalletService created`);
        this._encryptor = new AesEncryption();
        this._encryptor.importKey(signerSecret());
        this._isProd = isProd;
    }

    public get currentNetworkType(): SupportedNetwork | undefined {
        if (!this._chainId) {
            return undefined;
        }
        return getSupportedNetworkFromChainId(this._chainId.dec, this._isProd);
    }

    private readonly _encryptor: AesEncryption;

    private _onWalletDisconnect: Callback[] = [];
    private _walletAddress: string | null = null;
    private _chainId: ChainId | null = null;
    private _walletProvider: Provider | null = null;
    private _etherProvider: BrowserProvider | null = null;
    private _isLoggingOut = false;
    private _isConnected = false;
    private _isProd = false;

    async updateWalletAddress(walletAddress: string | undefined) {
        this._logger.log(`${TAG} updateWalletAddress to ${walletAddress}`);
        
        // User logout khỏi Wallet
        if (!walletAddress && this._walletAddress) {
            this._notificationService.showError("You're logging out of your wallet. Please log in again.");
            return;
        }

        // User đổi sang Account khác trên cùng Wallet
        if (walletAddress && this._walletAddress && walletAddress != this._walletAddress) {
            this._walletAddress = walletAddress!;
            this._notificationService.show("Account change", `You're connect to different account: ${walletAddress}`, 5);
            return;
        }

        if (walletAddress) {
            // User Login lần đầu
            if (!this._walletAddress) {
                this._logger.log(`${TAG} Wallet address ${this._walletAddress} updated to ${walletAddress}`);
                this._walletAddress = walletAddress!;
                this._notificationService.show('Wallet connected', `Welcome to Bomcrypto, ${this._walletAddress}`, 2, 'success');
            }

            this._isLoggingOut = false;
        }
    }

    async disconnectWallet() {
        this._onWalletDisconnect.forEach(cb => cb());
        this.clearWalletAddress();
        //this._walletProvider?.emit('disconnect');
        this._isLoggingOut = true;

    }

    async updateWalletProvider(walletProvider: Provider) {
        this._logger.log(`${TAG} updateWalletProvider`);
        if (!walletProvider) {
            return;
        }
        if (this._walletProvider != walletProvider) {
            this._logger.log(`${TAG} updateWalletProvider`);
            this._walletProvider?.removeListener('disconnect', this.onWalletDisconnected.bind(this));

            walletProvider.on('disconnect', this.onWalletDisconnected.bind(this));
            setProvider(walletProvider)
            this._walletProvider = walletProvider;

            this.updateNetwork().then();
        }
    }
    
    // Hàm này dùng để update lại network trên app-kit library đc gọi tử metamask nên ko cần gọi lại metamask để update network nữa
    public async updateAppKitNetwork(){
        this._logger.log(`${TAG} updateNetwork from metamask`);
        if (!this._walletProvider) {
            this._logger.error(`${TAG} Wallet provider is not set`);
            return;
        }
        this._etherProvider = new BrowserProvider(this._walletProvider);
        setBrowserProvider(this._etherProvider);
        const chainId = await getChainId(this._logger, this._etherProvider);
        if(chainId == null) {
            this._logger.error(`${TAG} Failed to get chain ID from provider`);
            return;
        }       
        this.setChainId(chainId);

    }

    // Function chỉ đc gọi khi đổi network bằng app-kit library nên sẽ cần force metamask update lại network
    public async updateNetwork() {
        this._logger.log(`${TAG} updateNetwork from app-kit`);
        if (!this._walletProvider) {
            this._logger.error(`${TAG} Wallet provider is not set`);
            return;
        }

        this._etherProvider = new BrowserProvider(this._walletProvider);
        setBrowserProvider(this._etherProvider);


        if (!this._chainId) {
            this._chainId = await getChainId(this._logger, this._etherProvider);
        }
        if (!this._chainId) {
            return;
        }

        // Đổi network trên wallet, yêu cầu đổi network trên metamask
        const chainId = await getChainId(this._logger, this._etherProvider);
        if (!chainId) {
            this._logger.error(`${TAG} Failed to get chain ID from provider`);
            this._notificationService.showError("Failed to get chain ID. Please check your wallet connection.");
            return;
        }
        if (this._chainId.dec == chainId.dec) {
            // ko đổi chain - ko sao
            return;
        }
        const swaped = await forceSwapChain(this._logger, this._etherProvider, this._chainId);
        if (swaped) {
            return;
        }
        const supportedNetwork = getSupportedNetworkFromChainId(this._chainId.dec);
        if (!supportedNetwork) {
            this._logger.error(`${TAG} Unsupported chain ID: ${this._chainId}`);
            this._notificationService.showError("Unsupported network. Please check your wallet connection.");
            return;
        }
        const rpc = getRpc(supportedNetwork, this._isProd);
        if (!rpc) {
            this._logger.error(`${TAG} No RPC found for chain ID: ${this._chainId}`);
            return;
        }
        const added = await addNetwork(this._logger, this._etherProvider, rpc);
        if (!added) {
            this._logger.error(`${TAG} Failed to add or switch network`);
            this._notificationService.showError("Failed to switch or add network. Please check your wallet connection.");
            return;
        }
        this.setChainId(chainId);
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

    updateWalletConnection(isConnected: boolean) {
        this._isConnected = isConnected;
        this._logger.log(`${TAG} updateWalletConnection ${isConnected}`);
        window.setConnectWallet(true);
        window.setConnectUser(isConnected);
        if (isConnected && typeof window.enableBgVideo === 'function') {
            window.enableBgVideo(false);
        }
    }

    async isReady(): Promise<boolean> {
        return await this.getWalletAddress() != null;
    }

    async getWalletAddress(): Promise<string> {
        while (this._walletAddress == null) {
            this._logger.log(`Waiting for update wallet address`);
            await sleep(1000);
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
            if (!network || (network !== 'ronin' && network !== 'base')) {
                logger.error(`${TAG} Unsupported network: ${network}. Only 'Ronin' and 'Base' are supported.`);

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
                showError(`Failed to switch to Ronin or Base network.`);
                return false;
            }
            return true;

        } catch (e) {
            logger.error(`${TAG} Error in forceSwapChain: ${(e as Error).message}`);
            showError("Failed to swap chain. Please try again.");
            return false;
        }
    }

    async transferCoin(sendToAddress: string, amountToSend: string | number): Promise<boolean> {
        const isProd = this._isProd;
        const provider = this._etherProvider;
        const logger = this._logger;
        const showNoti = this._notificationService.show.bind(this._notificationService);
        const showError = this._notificationService.showError.bind(this._notificationService);

        if (!sendToAddress || sendToAddress.length === 0 || !ethers.isAddress(sendToAddress)) {
            logger.error(`${TAG} Invalid sendToAddress: ${sendToAddress}`);
            showError("Invalid recipient address.");
            return false;
        }

        try {
            /**
             * Các bước:
             * 1. Lấy ra: địa chỉ ví, số dư đang có
             * 2. Kiểm tra số dư có đủ để gửi không
             * 3. Encode message (nếu có) sang hex
             */
            if (!provider) {
                logger.error(`${TAG} Ether provider is not set`);
                showError("Wallet provider is not connected. Please check your wallet connection.");
                return false;
            }

            // Bước 1: Kiểm tra thử Provider đang ở mạng nào, nếu ko phải Ronin hoặc Base thì yêu cầu Swap về mạng đó.
            const currentSelectedNetwork = await getChainId(logger, provider);
            if (!currentSelectedNetwork) {
                logger.error(`${TAG} Failed to get current selected network`);
                showError("Failed to get current network. Please check your wallet connection.");
                return false;
            }
            const network = getSupportedNetworkFromChainId(currentSelectedNetwork.dec, isProd);
            if (!network || (network !== 'ronin' && network !== 'base')) {
                logger.error(`${TAG} Unsupported network: ${network}. Only 'Ronin' and 'Base' are supported.`);
                const chosenChainId = this.getChainId();
                if (!chosenChainId) {
                    logger.error(`${TAG} No chainId found`);
                    showError("No chainId found. Please check your wallet connection.");
                    return false;
                }
                const swapped = await forceSwapChain(logger, provider, chosenChainId);
                if (!swapped) {
                    showError(`Failed to switch to Ronin or Base network.`);
                    return false;
                }
            }

            // Bước 2: Sau khi đã đúng mạng thì lấy wallet address 
            const senderAddress = await getWalletAddress(logger, provider);
            if (!senderAddress) {
                showError("Error happened. Please check your wallet connection.");
                return false;
            }

            // Bước 3: Sau đó lấy balance & kiểm tra số dư
            const balance = await getBalance(this._logger, provider);
            if (balance == null) {
                showError("Error happened! Please check your wallet connection.");
                return false;
            }

            let amountInWei: bigint;
            if (typeof amountToSend === 'number') {
                amountInWei = ethers.parseUnits(amountToSend.toString(), 'ether');
            } else {
                amountInWei = ethers.parseEther(amountToSend);
            }

            if (balance <= amountInWei) { // added gas fee
                logger.error(`${TAG} Insufficient balance: ${balance} < ${amountInWei}`);
                showError("Insufficient balance to transfer.");
                return false;
            }

            // Bước 4: Tạo transaction
            const result = await sendTransaction(
                logger, provider, {
                    to: sendToAddress,
                    value: amountInWei,
                });
            if (!result.success) {
                if (result.error !== 'denied') {
                    showError(`Failed to transfer coin: ${result.error}`);
                }
                return false;
            }
            showNoti(`Transaction sent`, `Transaction hash: ${result.txHash}`, 5, 'success');
            logger.log(`${TAG} Transaction sent successfully: ${result.txHash}`);
            return true;

        } catch (e) {
            logger.error(`${TAG} Error in transferCoin: ${(e as Error).message}`);
            showError("Failed to create transfer. Please try again.");
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

    setOnWalletDisconnected(onWalletDisconnect: Callback) {
        this._onWalletDisconnect.push(onWalletDisconnect);
    }

    private clearWalletAddress() {
        this._walletAddress = null;
    }

    private onWalletDisconnected() {
        if (this._isLoggingOut) {
            return;
        }

        this._isLoggingOut = true;
        this.clearWalletAddress();
        unityBridge.reloadClient();
        this._onWalletDisconnect.forEach(cb => cb());
        this._logger.log(`${TAG} onWalletDisconnected`);
        this._notificationService.showError("You're logging out of your wallet.");
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

type Callback = () => void;

export type Account = {
    userName: string;
    password: string;
}
export type HandshakeType = "Login" | "Reconnect"

export type ReadyToSign = {
    walletAddress: string;
    chainId: ChainId;
    network: SupportedNetwork;
    isProd: boolean;
}