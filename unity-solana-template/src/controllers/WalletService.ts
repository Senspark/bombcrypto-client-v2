import Logger from "./Logger.ts";
import {Provider} from "@reown/appkit-adapter-solana";
import {sleep} from "../utils/Time.ts";
import {
    Connection,
    LAMPORTS_PER_SOL,
    PublicKey,
    SystemProgram,
    Transaction,
    TransactionSignature
} from "@solana/web3.js";

import {createTransferInstruction} from "@solana/spl-token";
import NotificationService from "./NotificationService.ts";
import AesEncryption from "./encrypt/AesEncryption.ts";
import {base64ToByteArray, byteArrayToBase64, byteArrayToUint32, stringToByteArray} from "../utils/String.ts";
import unityBridge from "./unity/UnityBridge.ts";
import {EnvConfig} from "../configs/EnvConfig.ts";
import {createMemoInstruction} from "@solana/spl-memo";

const TAG = '[WS]';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-expect-error
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const MEMO_PROGRAM_ID = new PublicKey("MemoSq4gqABAXKb96qnH8TysNcWxMyWCqXgDLGmfcHr");
const TOKEN_PROGRAM_ID = new PublicKey('TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA');
const ASSOCIATED_TOKEN_PROGRAM_ID = new PublicKey('ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL');

export default class WalletService {
    constructor(
        _isProd: boolean,
        signerSecret: () => string,
        private readonly _signPadding: () => string,
        private readonly _logger: Logger,
        private readonly _notificationService: NotificationService
    ) {
        _logger.log(`${TAG} new WalletService created`);
        this._encryptor = new AesEncryption();
        this._encryptor.importKey(signerSecret());
        const linkSuffix = _isProd ? '' : '?cluster=devnet-solana';
        this._linkExplorer = `https://solana.fm/tx/@${linkSuffix}`;

        this._merchantAddress = EnvConfig.merchantAddress();
        this._bcoinAddress = new PublicKey(EnvConfig.bcoinAddress());
    }

    private readonly _linkExplorer: string;
    private readonly _merchantAddress: string;
    private readonly _bcoinAddress: PublicKey;
    private readonly _encryptor: AesEncryption;

    private _onWalletDisconnect: Callback[] = [];
    private _walletAddress: string | null = null;
    private _walletProvider: Provider | null = null;
    private _connection: Connection | null = null;
    private _isWrongWallet = false;

    updateWalletAddress(walletAddress: string | undefined) {
        this._logger.log(`${TAG} updateWalletAddress to ${walletAddress}`);

        // User logout khỏi Wallet
        if (!walletAddress && this._walletAddress) {
            if (!this._isWrongWallet) {
                this._notificationService.showError("You're logging out of your wallet. Please log in again.");
                this._isWrongWallet = true;
            }
            return;
        }

        // User đổi sang Account khác trên cùng Wallet
        if (walletAddress && this._walletAddress && walletAddress != this._walletAddress) {
            if (!this._isWrongWallet) {
                this._isWrongWallet = true;
                this._notificationService.showError("You're logging in with a different wallet. Please log in again.");
            }
            return;
        }

        if (walletAddress) {
            // User chọn lại Account cũ
            if (this._isWrongWallet) {
                this._isWrongWallet = false;
                this._notificationService.show('Wallet connected', `Welcome back ${this._walletAddress}`, 0, 'success');
            }

            // User Login lần đầu
            if (!this._walletAddress) {
                this._logger.log(`${TAG} Wallet address ${this._walletAddress} updated to ${walletAddress}`);
                this._walletAddress = walletAddress!;
                this._notificationService.show('Wallet connected', `Welcome to Bomcrypto, ${this._walletAddress}`, 2, 'success');
            }
        }
    }

    async disconnectWallet() {
        await this._walletProvider?.disconnect();
    }

    updateWalletProvider(walletProvider: Provider) {
        if (this._walletProvider != walletProvider) {
            this._logger.log(`${TAG} updateWalletProvider`);
            this._walletProvider = walletProvider;
            this._walletProvider?.removeListener('disconnect', this.onWalletDisconnected.bind(this));
            this._walletProvider?.on('disconnect', this.onWalletDisconnected.bind(this));
        }
    }

    updateWalletConnection(connection: Connection | undefined) {
        this._connection = connection ?? null;
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
            if (!this._walletProvider) {
                this._logger.error(`Wallet provider is not set`);
                return null;
            }
            const message = await this.generateStringToSign(nonce);
            const signedMessage: Uint8Array = await this._walletProvider.signMessage(stringToByteArray(message));
            return Buffer.from(signedMessage).toString('base64');
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    /**
     * @return transaction link
     */
    async depositSol(depositMessage: string, solAmount: number): Promise<string | null> {
        try {
            if (!this._walletProvider) {
                this._logger.error(`Wallet provider is not set`);
                return null;
            }
            if (!this._connection) {
                this._logger.error(`Cannot connect to Solana network`);
                return null;
            }
            const fromAddress = new PublicKey(await this.getWalletAddress());
            const toAddress = new PublicKey(this._merchantAddress);
            const lamports = Math.floor(LAMPORTS_PER_SOL * solAmount);

            const balance = await this._connection.getBalance(fromAddress);
            this._logger.log(`Your Balance: ${balance / LAMPORTS_PER_SOL} SOL`);
            if (balance < lamports) {
                this._logger.error(`Insufficient balance`);
                this._notificationService.showError(`Insufficient balance, you need at least ${solAmount} SOL in your account.`);
                return null;
            }

            const latestBlockchain = await this._connection.getLatestBlockhash("confirmed");
            const transaction = new Transaction().add(
                createMemoInstruction(depositMessage),
                SystemProgram.transfer({
                    fromPubkey: fromAddress,
                    toPubkey: toAddress,
                    lamports: lamports,
                })
            );

            transaction.recentBlockhash = latestBlockchain.blockhash;
            transaction.feePayer = fromAddress;

            const tx: TransactionSignature = await this._walletProvider.signAndSendTransaction(transaction);
            const linkExplorer = this._linkExplorer.replace('@', tx);
            this._logger.log(`Transaction link: ${linkExplorer}`);
            return linkExplorer;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    /**
     * @return transaction link
     */
    async depositBcoin(depositMessage: string, bcoinAmount: number): Promise<string | null> {
        try {
            if (!this._walletProvider) {
                this._logger.error(`Wallet provider is not set`);
                return null;
            }
            if (!this._connection) {
                this._logger.error(`Cannot connect to Solana network`);
                return null;
            }

            const fromAddress = new PublicKey(await this.getWalletAddress());
            const toAddress = new PublicKey(this._merchantAddress);
            if (fromAddress.equals(toAddress)) {
                this._logger.error(`Cannot send Bcoin to yourself`);
                return null;
            }
            
            const lamports = Math.floor(LAMPORTS_PER_SOL * bcoinAmount);

            const balance = await this.getBcoinBalance(fromAddress);
            if (!balance) {
                this._logger.error(`Cannot get Bcoin balance`);
                return null;
            }
            const [balanceBig, balanceDec] = balance;
            this._logger.log(`Your Balance: ${balanceDec} BCOIN`);
            if (balanceBig < lamports) {
                this._logger.error(`Insufficient balance`);
                this._notificationService.showError(`Insufficient balance, you need at least ${bcoinAmount} BCOIN in your account.`);
                return null;
            }

            const latestBlockchain = await this._connection.getLatestBlockhash("confirmed");
            const transaction = new Transaction().add(
                createMemoInstruction(depositMessage),
                createTransferInstruction(
                    this.getTokenAccount(fromAddress),
                    this.getTokenAccount(toAddress),
                    fromAddress,
                    lamports
                )
            );

            transaction.recentBlockhash = latestBlockchain.blockhash;
            transaction.feePayer = fromAddress;

            const tx: TransactionSignature = await this._walletProvider.signAndSendTransaction(transaction);
            const linkExplorer = this._linkExplorer.replace('@', tx);
            this._logger.log(`Transaction link: ${linkExplorer}`);
            return linkExplorer;
        } catch (e) {
            this._logger.error(`Error: ${(e as Error).message}`);
            return null;
        }
    }

    async getBcoinBalance(wallet: PublicKey | string | null = null): Promise<[bigint, number] | null> {
        try {
            if (!this._walletProvider) {
                this._logger.error(`Wallet provider is not set`);
                return null;
            }
            if (!this._connection) {
                this._logger.error(`Cannot connect to Solana network`);
                return null;
            }
            let fromAddress: PublicKey;
            if (!wallet) {
                fromAddress = new PublicKey(await this.getWalletAddress());
            } else if (typeof wallet === 'string') {
                fromAddress = new PublicKey(wallet);
            } else {
                fromAddress = wallet;
            }
            const address = this.getTokenAccount(fromAddress);
            const tokenAmount = await this._connection.getTokenAccountBalance(address);
            const big = BigInt(tokenAmount.value.amount);
            const dec = lamportsBigIntToBcoin(big);
            return [big, dec];
        } catch (e) {
            this._logger.error((e as Error)?.message);
            return null;
        }
    }

    private getTokenAccount(walletAddr: PublicKey): PublicKey {
        if (!this._connection) {
            throw new Error(`Cannot connect to Solana network`);
        }
        const [address] = PublicKey.findProgramAddressSync(
            [walletAddr.toBuffer(), TOKEN_PROGRAM_ID.toBuffer(), this._bcoinAddress.toBuffer()],
            ASSOCIATED_TOKEN_PROGRAM_ID
        );
        return address;
    }

    setOnWalletDisconnected(onWalletDisconnect: Callback) {
        this._onWalletDisconnect.push(onWalletDisconnect);
    }

    private clearWalletAddress() {
        this._walletAddress = null;
    }

    private onWalletDisconnected() {
        if (this._walletAddress == null) {
            return;
        }
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

function lamportsBigIntToBcoin(lamports: bigint, decimal: number = 9): number {
    return Number(lamports) / 10 ** decimal;
}