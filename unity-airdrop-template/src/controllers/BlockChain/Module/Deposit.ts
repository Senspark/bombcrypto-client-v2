import GeneralContract from './Utils/GeneralContract.js';
import * as NetworkUtils from "./Utils/NetworkUtils.js";
import {Contract, parseUnits} from "ethers";

export default class Deposit extends GeneralContract {
    /**
     *
     * @param {string} userAddress
     * @param {number} amount
     * @param {number} category
     * @returns {Promise<boolean>}
     */
    async deposit(userAddress: string, amount: number, category: number): Promise<boolean> {
        try {
            const token = NetworkUtils.getCoinTokenByBuyHeroCategory(category);
            // const tokenAddress = token._address;
            const amountFormatted = parseUnits(amount.toString(), 18);

            // Check allowance.
            await token.approveMaximum(userAddress, this._address, amountFormatted);

            const contract: Contract = await this.getContract();
            const estimateGas = await contract.deposit.estimateGas(amountFormatted);
            const options = NetworkUtils.getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.deposit(amountFormatted, options);
            await NetworkUtils.waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    /**
     *
     * @param {string} userAddress
     * @param {number} amount
     * @param {number} category
     * @returns {Promise<boolean>}
     */
    async depositV2(userAddress: string, amount: number, category: number): Promise<boolean> {
        try {
            const token = NetworkUtils.getCoinTokenByBuyHeroCategory(category);
            const tokenAddress = token._address;
            const amountFormatted = parseUnits(amount.toString(), 18);

            // Check allowance.
            await token.approveMaximum(userAddress, this._address, amountFormatted);

            const contract: Contract = await this.getContract();
            const estimateGas = await contract.depositV3.estimateGas(amountFormatted, tokenAddress);
            const options = NetworkUtils.getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.depositV3(amountFormatted, tokenAddress, options);
            await NetworkUtils.waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }
}