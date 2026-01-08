import {ReadyToSign} from "./WalletService.ts";
import NotificationService from "./NotificationService.ts";
import Logger from "./Logger.ts";
import {getBalance} from "./WalletUtils.ts";
import {ethers} from 'ethers'
import {SupportedNetwork} from "./RpcNetworkUtils.ts";
import WalletService from "./WalletService.ts";

/**
 * Phải fetch từ server về
 */
const MerchantAddressTest = new Map<SupportedNetwork, string>([
    ['ronin', ''],
    ['base', ''],
    ['vic', ''],
    ['bsc', ''],
    ['polygon', '']
]);
const MerchantAddressMain = new Map<SupportedNetwork, string>([
    ['ronin', '0x51afe7518f837f44e1514ac9a8bc0f8405bc14b6'],
    ['base', '0xf3a7195920519f8A22cDf84EBB9F74342abE9812'],
    ['vic', '0xf3a7195920519f8A22cDf84EBB9F74342abE9812'],
    ['bsc', ''],
    ['polygon', '']
]);

const DEPOSIT_ABI = [
    'function deposit(string invoice) external payable'
]

/**
 * Dành cho deposit theo invoice: RON & BASE
 */
export default class DepositServiceWithInvoice {

    constructor(
        private readonly _logger: Logger,
        private readonly _walletService: WalletService,
        private readonly _notiService: NotificationService,
    ) {
    }

    async deposit(amountToSend: string, invoice: string): Promise<boolean> {
        const logger = this._logger;
        const showNoti = (message: string) => this._notiService.show('Success', message, 5, 'success');
        const showError = this._notiService.showError.bind(this._notiService);

        try {
            const ready = await this._walletService.isReadyToSign();
            if (!ready) {
                return false;
            }
            const provider = this._walletService.getWalletProvider()!;

            // Kiểm tra balance và số dư
            const balance = await getBalance(logger, provider);
            if (balance == null) {
                showError("Error happened! Please check your wallet connection.");
                return false;
            }

            const amountInWei = ethers.parseEther(amountToSend);

            if (balance <= amountInWei) { // added gas fee
                logger.error(`Insufficient balance: ${balance} < ${amountInWei}`);
                showError("Insufficient balance to transfer.");
                return false;
            }

            const merchantAddress = this.getMerchantAddress(ready);
            if (!merchantAddress) {
                logger.error(`Merchant address not found for network: ${ready.network}`);
                showError("Merchant address not found for this network.");
                return false;
            }

            const signer = await provider.getSigner();
            const contract = new ethers.Contract(merchantAddress, DEPOSIT_ABI, signer);
            const tx: ContractTransactionResponse = await contract.deposit(invoice, {value: amountInWei})
            logger.log(`Deposit transaction hash: ${tx.hash}`);
            showNoti(`Deposit transaction hash send: ${this._walletService.getBlockExplorerUrl(tx.hash)}`);

            // Wait for transaction confirmation
            const receipt: ContractTransactionReceipt = await tx.wait();
            logger.log(`Deposit transaction confirmed: ${receipt.hash}`);

            return receipt.status == 1;
        } catch (ex) {
            logger.error(`Deposit error: ${ex}`)
            return false;
        }
    }

    private getMerchantAddress(readyData: ReadyToSign): string | undefined {
        const network = readyData.network;
        const address = readyData.isProd ? MerchantAddressMain.get(network) : MerchantAddressTest.get(network);
        if (address && address.length > 0) {
            return address;
        }
        return undefined;
    }
}

type ContractTransactionResponse = {
    hash: string;
    to: string;
    from: string;
    nonce: number;
    data: string;
    value: bigint;
    chainId: bigint;
    wait: (confirms?: number, timeout?: number) => Promise<ContractTransactionReceipt>;
};

type ContractTransactionReceipt = {
    to: string;
    from: string;
    hash: string;
    status: number; // 1 for success, 0 for failure
};