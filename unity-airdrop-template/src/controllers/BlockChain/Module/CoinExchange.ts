import GeneralContract from "./Utils/GeneralContract.js";
import * as NetworkUtils from './Utils/NetworkUtils.js';
import * as Message from './Utils/Message.js';
import CoinToken from "./CoinToken.js";
import {Contract, parseUnits} from "ethers";

export default class CoinExchange extends GeneralContract {
    private _coinToken: CoinToken;
    private _otherToken: CoinToken;

    /**
     * @param {string} address
     * @param {any} abi
     * @param coinToken
     * @param otherToken
     */
    constructor(address: string, abi: JSON, coinToken: CoinToken, otherToken: CoinToken) {
        super(address, abi);
        this._coinToken = coinToken;
        this._otherToken = otherToken;
    }

    /**
     * @return {Promise<{price: number, slippage: number, fee: number}>} [{price, slippage, fee}]
     */
    async getInfoBuy(): Promise<{ price: number, slippage: number, fee: number }> {
        const contract: Contract = await this.getContract();
        const data = await contract.getInfoBuy();
        const price = data[0].toNumber() / 10 ** 6;
        const slippage = data[1].toNumber() / 1000 * 100;
        const fee = data[2].toNumber() / 1000 * 100;
        return { price, slippage, fee };
    }

    /**
     * @param amount
     * @param category
     * @param {string} userAddress
     * @return {Promise<Message.Message>}
     */
    async buyTokens(amount: number, category: number, userAddress: string): Promise<Message.Message> {
        try {
            const amountBN = this.parseAmount(amount, category);
            await this._otherToken.approveMaximum(userAddress, this._address, amountBN);

            const contract: Contract = await this.getContract();
            const tokenAddress: string = this.getTokenAddress(category);
            const estimateGas = await contract.buyTokensV2.estimateGas(amountBN, tokenAddress);
            const options = NetworkUtils.getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.buyTokensV2(amountBN, tokenAddress, options);

            await NetworkUtils.waitForReceipt(transaction);
            //FIXME: Js cũ ko có tham số nên ko biết để cái gì
            return Message.Info("Buy success");
        } catch (ex) {
            console.error(`exception ${ex}`);
            return Message.Error("Buy fail");
            //return Message.Error(ex.message.substring(0, 100));
        }
    }

    /**
     * @return {BigNumber}
     * @param amount
     * @param category
     */
    parseAmount(amount: number, category: number): bigint {
        const digit = category === 0 ? 6 : 18;
        return parseUnits(amount.toString(), digit);
    }

    /**
     * @return {string}
     * @param category
     */
    getTokenAddress(category: number): string {
        const token = category === 0 ? this._otherToken : this._coinToken;
        return token._address;
    }
}