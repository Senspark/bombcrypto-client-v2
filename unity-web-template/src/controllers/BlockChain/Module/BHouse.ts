import NFTToken from "./Utils/NFTToken.js";
import { convertBN2DArrayToNumber2DArray, convertBNArrayToNumberArray } from "./Utils/Utils.js";
import { getDoubleGasFeeOption, waitForReceipt } from "./Utils/NetworkUtils.js";
import { Contract } from "ethers";
import CoinToken from "./CoinToken.ts";

export default class BHouseToken extends NFTToken {
    private _bcoinToken: CoinToken;

    constructor(bcoinToken: CoinToken, address: string, abi: JSON, designAbi: JSON) {
        super(address, abi, designAbi);
        this._bcoinToken = bcoinToken;
    }

    async getRarityStats(): Promise<string[][]> {
        const contract: Contract = await this.getDesignContract();
        const value = await contract.getRarityStats();
        const result = convertBN2DArrayToNumber2DArray(value);
        return result;
    }

    async getTokenLimit(): Promise<string> {
        const contract: Contract = await this.getDesignContract();
        const value = await contract.getTokenLimit();
        return value.toString();
    }

    async getMintCost(rarity: number): Promise<string> {
        const contract: Contract = await this.getDesignContract();
        const value = await contract.getMintCost(rarity);
        return value.toString();
    }

    async getMintCosts(): Promise<string[]> {
        const contract: Contract = await this.getDesignContract();
        const value = await contract.getMintCosts();
        const result = convertBNArrayToNumberArray(value);
        return result;
    }

    async getMintAvailable(): Promise<string[]> {
        const contract: Contract = await this.getContract();
        const value = await contract.getMintAvailable();
        const result = convertBNArrayToNumberArray(value);
        return result;
    }

    async getMintLimits(): Promise<string[]> {
        const contract: Contract = await this.getDesignContract();
        const value = await contract.getMintLimits();
        const result = convertBNArrayToNumberArray(value);
        return result;
    }

    async getTokenDetails(userAddress: string): Promise<string> {
        const contract: Contract = await this.getContract();
        const value = await contract.getTokenDetailsByOwner(userAddress);
        return value;
    }

    async mint(userAddress: string, rarity: number): Promise<boolean> {
        try {
            const cost = await this.getMintCost(rarity);
            await this._bcoinToken.checkAllowance(userAddress, this._address, BigInt(cost));

            const contract: Contract = await this.getContract();
            const estimateGas = await contract.mint.estimateGas(rarity);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.mint(rarity, options);
            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }
}