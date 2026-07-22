import GeneralContract from './Utils/GeneralContract.js';
import { waitForReceipt } from "./Utils/NetworkUtils.js";
import { parseEther, formatUnits} from "ethers";
import CoinToken from "./CoinToken.ts";

export default class BHeroStake extends GeneralContract {
    private readonly _bcoinToken: CoinToken;
    private readonly _sensparkToken: CoinToken;

    constructor(bcoinToken: CoinToken, sensparkToken: CoinToken, address: string, abi: JSON) {
        super(address, abi);
        this._bcoinToken = bcoinToken;
        this._sensparkToken = sensparkToken;
    }

    async checkAllowance(userAddress: string, amount: bigint): Promise<void> {
        const address = this._address;
        await this._bcoinToken.checkAllowance(userAddress, address, amount);
    }

    async depositCoinIntoHeroId(userAddress: string, id: number, amount: number): Promise<boolean> {
        try {
            const amountBN = parseEther(amount.toString());
            await this._bcoinToken.approveMaximum(userAddress, this._address, amountBN);
            const contract = await this.getContract();
            const transaction = await contract.depositCoinIntoHeroId(id, amountBN);
            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async withdrawCoinFromHeroId(userAddress: string, id: number, amount: number): Promise<boolean> {
        try {
            const amountBN = parseEther(amount.toString());
            await this._bcoinToken.approveMaximum(userAddress, this._address, amountBN);
            const contract = await this.getContract();
            const transaction = await contract.withdrawCoinFromHeroId(id, amountBN);
            await waitForReceipt(transaction);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }

    async getCoinBalancesByHeroId(id: number): Promise<string> {
        try {
            const contract = await this.getContract();
            const result = await contract.getCoinBalancesByHeroId(id);
            return formatUnits(result.toString(), 18);
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "0";
        }
    }

    async getWithdrawFeeByHeroId(id: number): Promise<string> {
        try {
            const contract = await this.getContract();
            const result = await contract.getWithdrawFeeByHeroId(id);
            return result.toString();
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "0";
        }
    }

    private getCoinTokenByCategory(category: number): CoinToken | null {
        if (category === 0) return this._bcoinToken;
        if (category === 1) return this._sensparkToken;
        return null;
    }

    async depositV2(userAddress: string, id: number, amount: number, category: number): Promise<{ success: boolean; txHash: string }> {
        try {
            const amountBN = parseEther(amount.toString());
            const coinToken = this.getCoinTokenByCategory(category);
            if (!coinToken) {
                console.error(`coinToken is null for category ${category}`);
                return { success: false, txHash: "" };
            }
            await coinToken.approveMaximum(userAddress, this._address, amountBN);
            const contract = await this.getContract();
            const transaction = await contract.depositV2(coinToken._address, id, amountBN);
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "" };
        }
    }

    async withdrawV2(id: number, amount: number, category: number): Promise<{ success: boolean; txHash: string }> {
        try {
            const amountBN = parseEther(amount.toString());
            const coinToken = this.getCoinTokenByCategory(category);
            if (!coinToken) {
                console.error(`coinToken is null for category ${category}`);
                return { success: false, txHash: "" };
            }
            const contract = await this.getContract();
            const transaction = await contract.withdrawV2(coinToken._address, id, amountBN);
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "" };
        }
    }

    async getCoinBalanceV2(id: number, category: number): Promise<string> {
        try {
            const coinToken = this.getCoinTokenByCategory(category);
            if (!coinToken) {
                console.error(`coinToken is null for category ${category}`);
                return "0";
            }
            const contract = await this.getContract();
            const result = await contract.getCoinBalanceV2(coinToken._address, id);
            return formatUnits(result.toString(), 18);
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "0";
        }
    }

    async getWithdrawFeeV2(id: number, category: number): Promise<string> {
        try {
            const coinToken = this.getCoinTokenByCategory(category);
            if (!coinToken) {
                console.error(`coinToken is null for category ${category}`);
                return "0";
            }
            const contract = await this.getContract();
            const result = await contract.getWithdrawFeeV2(coinToken._address, id);
            return result.toString();
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "0";
        }
    }
}