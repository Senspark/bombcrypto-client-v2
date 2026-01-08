import GeneralContract from './Utils/GeneralContract.js';
import { getDoubleGasFeeOption, waitForReceipt } from "./Utils/NetworkUtils.js";
import { Contract } from "ethers";

export default class BHeroExtended extends GeneralContract {
    async getClaimableTokens(userAddress: string): Promise<string> {
        const contract: Contract = await this.getContract();
        const amount: bigint = await contract.getClaimableTokens(userAddress);
        return amount.toString();
    }

    async claim(): Promise<boolean> {
        try {
            const contract: Contract = await this.getContract();
            const estimateGas: bigint = await contract.claim.estimateGas();
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.claim(options);
            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }
}