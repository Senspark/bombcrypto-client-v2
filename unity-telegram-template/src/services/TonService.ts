import {TonConnectUI} from "@tonconnect/ui-react";
import Logger from "./Logger.ts";
import {Address, beginCell, Cell, fromNano, toNano} from "@ton/core";
import {sleep} from "./Utils.ts";
import MerchantApiService from "../apis/MerchantApiService.ts";
import NotificationService from "./NotificationService.ts";
import {toNumberOrZero} from "../utils/Number.ts";
import {authApi} from "./GlobalServices.ts";

const K_HOST = `@HOST@`;
const K_MY_WALLET_ADDRESS = `@MY_WALLET_ADDRESS@`;
const K_BCOIN_ADDRESS = `@BCOIN_ADDRESS@`;
const K_TX = `@TX@`;
const GET_JETTON_WALLET_API = `https://${K_HOST}/api/v3/jetton/wallets?owner_address=${K_MY_WALLET_ADDRESS}&jetton_address=${K_BCOIN_ADDRESS}&limit=1&offset=0`
const GET_TX_API_TEST = `https://testnet.tonapi.io/v2/blockchain/transactions/${K_TX}`;
const VIEW_TX_API_TEST = `https://testnet.tonviewer.com/transaction/${K_TX}`;

const GET_TX_API_PROD = `https://tonapi.io/v2/blockchain/transactions/${K_TX}`;
const VIEW_TX_API_PROD = `https://tonviewer.com/transaction/${K_TX}`;

export default class TonService {

    constructor(
        logger: Logger,
        isProd: boolean,
        private readonly _merchantApi: MerchantApiService,
        private readonly _notificationService: NotificationService,
    ) {
        this._logger = logger.clone('[TON_SERVICE]');
        this._tonApi = isProd ? 'toncenter.com' : 'testnet.toncenter.com';
        
        this._getTxApi = isProd ? GET_TX_API_PROD : GET_TX_API_TEST;
        this._viewTxApi = isProd ? VIEW_TX_API_PROD : VIEW_TX_API_TEST;
    }
    
    private readonly _getTxApi: string;
    private readonly _viewTxApi: string;

    private _tonConnect: TonConnectUI | null = null;
    private readonly _logger: Logger;
    private readonly _tonApi: string;

    setTonConnect(tonConnect: TonConnectUI): void {
        this._tonConnect = tonConnect;
    }

    isConnected(): boolean {
        return this._tonConnect?.connected ?? false;
    }

    async transferTon(amount: number, invoiceId: string): Promise<boolean> {
        try {
            const transferToAddress = (await this._merchantApi.getAddresses())?.transferTonTo;
            if (!transferToAddress) {
                this._logger.error(`transferTon: transferToAddress is not available`);
                return false;
            }
            const body = beginCell().storeUint(0, 32).storeStringTail(invoiceId).endCell();
            const tx = await this.sendTransaction({
                address: transferToAddress,
                amount: toNano(amount).toString(),
                payload: body.toBoc().toString('base64'),
            });
            const viewLink = this._viewTxApi.replace(K_TX, tx);
            this._notificationService.showDepositSuccess(viewLink);
            return true;
        } catch (e) {
            this._logger.error(`transferTon error: ${(e as Error).message}`);
            return false;
        }
    }

    async transferBcoin(amount: number, invoiceId: string): Promise<boolean> {
        try {
            const address = await this._merchantApi.getAddresses();
            if (!address) {
                this._logger.error(`transferBcoin: address is not available`);
                return false;
            }
            const transferToAddress = Address.parse(address.transferTonTo);
            const myAddress = Address.parse(this.getMyAddress());
            const myJattonData = await this.getMyJettonAddress();
            const myJettonAddress = myJattonData.address;
            const balance = myJattonData.balance;
            
            //check if the balance is enough
            if (balance < amount) {
                this._notificationService.showError('Insufficient balance');
                return false;
            }

            const forwardPayload = beginCell()
                .storeUint(0, 32) // 0 opcode means we have a comment
                .storeStringTail(invoiceId)
                .endCell();

            const cellBuilder = beginCell();
            cellBuilder
                .storeUint(0x0f8a7ea5, 32) // opcode for JettonTransfer
                .storeUint(0, 64) // query id
                .storeCoins(toNano(amount)) // auto convert to 10**9
                .storeAddress(transferToAddress) // Address of the new owner of the jettons
                .storeAddress(myAddress) // Wallet address used to return remained ton coins with excesses message.
                .storeBit(0) // no custom payload
                .storeCoins(1) // forward amount - if > 0, will send notification message
                .storeBit(1) // store forward payload as a reference
                .storeRef(forwardPayload)
            ;

            const bodyMessage = cellBuilder.endCell();

            const tx = await this.sendTransaction({
                address: myJettonAddress,
                amount: toNano('0.1').toString(),
                payload: bodyMessage.toBoc().toString('base64'),
            });

            const viewLink = this._viewTxApi.replace(K_TX, tx);
            this._notificationService.showDepositSuccess(viewLink);
            return true;
        } catch (e) {
            this._logger.error(`transferBcoin error: ${(e as Error).message}`);
            return false;
        }
    }

    async logout(): Promise<void> {
        await this._tonConnect?.disconnect();
    }

    private async sendTransaction(message: CreateMessage): Promise<string> {
        const result = await this._tonConnect?.sendTransaction({
            messages: [message],
            validUntil: Date.now() + 5 * 60 * 1000, // 5 minutes for user to approve
        }, {
            modals: 'all',
        });
        if (!result) {
            throw new Error('Failed to send transaction');
        }
        if ('error' in result) {
            throw new Error((result as unknown as SendTransactionResponseError).error.message);
        }
        return await this.waitTx(result?.boc);
    }

    private async getMyJettonAddress(): Promise<UserWalletData> {
        const addresses = await this._merchantApi.getAddresses();
        if (!addresses) {
            throw new Error('Failed to get addresses');
        }

        const fetcher = async (url: string): Promise<UserWalletData> => {
            const res = await fetch(url);
            if (res.ok) {
                const jet = await res.json() as JettonWalletsResponse;
                const addr = jet?.jetton_wallets.at(0)?.address ?? null;
                const bal = jet?.jetton_wallets.at(0)?.balance ?? null;

                return {
                    address: Address.parse(addr ?? '').toRawString(),
                    balance: toNumberOrZero(fromNano(bal ?? '0')),
                };
            }
            throw new Error('Failed to get Jetton Wallet Address');
        }

        try {
            const myAddress = this.getMyAddress();
            const url = GET_JETTON_WALLET_API
                .replace(K_HOST, this._tonApi)
                .replace(K_MY_WALLET_ADDRESS, myAddress)
                .replace(K_BCOIN_ADDRESS, addresses.bcoinAddress);
            this._logger.log(`Requesting Jetton Wallet Address: ${url}`);
            return await fetcher(url);
        } catch (e) {
            this._logger.error(`getMyJettonAddress error: ${(e as Error).message}`);
            throw new Error('Failed to get Jetton Wallet Address');
        }
    }

    private getMyAddress(): string {
        return this._tonConnect?.account?.address ?? '';
    }

    private async waitTx(boc: string | undefined): Promise<string> {
        if (!boc) {
            return '';
        }
        this._logger.log(`boc: ${boc}`);
        const tx = Cell.fromBase64(boc).hash().toString('hex');
        const url = this._getTxApi.replace(K_TX, tx);
        const maxTried = 5;
        const sleepTime = 5000;
        let tried = 0;
        this._logger.log(`Waiting for transaction: ${url}`);

        while (true) {
            tried++
            try {
                const res = await fetch(url);
                if (!res.ok) {
                    await sleep(sleepTime);
                    continue;
                }

                const txRes = await res.json() as TxResult;
                if (txRes && txRes.success) {
                    return tx;
                } else {
                    if (tried >= maxTried) {
                        return tx;
                    }
                }
            } catch (e) {
                this._logger.error(`waitTx error: ${(e as Error).message}`);
            }
            await sleep(sleepTime);
        }
    }

    async getTonBalance(): Promise<number> {
        try {
            // Get the wallet address
            const myAddress = authApi.getAccountTon().address
            if (!myAddress) {
                this._logger.log('No wallet address found');
                return 0;
            }
            
            try {
                const url = `https://${this._tonApi}/api/v2/getAddressBalance?address=${myAddress}`;
                
                this._logger.log(`Requesting balance from: ${url}`);
                const response = await fetch(url, {
                    method: 'GET',
                });
                
                if (response.ok) {
                    const responseData = await response.json();
                    this._logger.log(`Balance response: ${JSON.stringify(responseData)}`);
                    
                    // The balance should be in result field
                    if (responseData.ok && responseData.result) {
                        const balanceNano = responseData.result;
                        this._logger.log(`Balance in nanoTON: ${balanceNano}`);
                        
                        // Convert from nanoTON to TON
                        return parseFloat(fromNano(balanceNano));
                    }
                    
                    this._logger.error(`No balance found in response: ${JSON.stringify(responseData)}`);
                } else {
                    this._logger.error(`Balance API error: ${response.status} ${response.statusText}`);
                }
            } catch (e) {
                this._logger.error(`getTonBalance API error: ${(e as Error).message}`);
            }
            
            // Fallback method if API call fails
            // if (this._tonConnect && this._tonConnect.account) {
            //     // Using proper type checking instead of assertion
            //     const accountObj = this._tonConnect.account;
            //     // Try to get the balance if it exists in the account object
            //     if ('balance' in accountObj && typeof accountObj.balance === 'string') {
            //         return parseFloat(fromNano(accountObj.balance));
            //     }
            // }
            
            this._logger.log('Could not retrieve TON balance');
            return 0;
        } catch (e) {
            this._logger.error(`getTonBalance error: ${(e as Error).message}`);
            return 0;
        }
    }
}

interface SendTransactionResponseError {
    error: { code: number; message: string };
    id: string;
}

type UserWalletData = {
    address: string;
    balance: number;
}

type JettonWalletsResponse = {
    jetton_wallets: {
        address: string;
        balance: string;
    }[];
}

type TxResult = {
    success: boolean,
}

type CreateMessage = {
    address: string;
    amount: string;
    stateInit?: string | undefined;
    payload?: string | undefined;
}

