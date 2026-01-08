import GeneralContract from './Utils/GeneralContract.js';
import { ethers, MaxUint256, parseUnits} from "ethers";

export default class CoinToken extends GeneralContract {
    async getBalance(userAddress: string): Promise<string> {
        const contract = await this.getContract();
        if (!contract) {
            throw new Error('Contract is not initialized');
        }
        const balance = await contract.balanceOf(userAddress);
        return balance.toString();
    }

    /**
     * @param userAddress - The address of the user
     * @param spenderAddress - The address of the spender
     * @param amount - The amount to approve
     * @returns A promise that resolves to a boolean indicating success
     */
    async approveMaximum(userAddress: string, spenderAddress: string, amount: bigint): Promise<boolean> {
        const allowance = await this.getAllowance(userAddress, spenderAddress);
        if (amount <= allowance) {
            return true;
        }
        const maxAmount = MaxUint256;
        const contract = await this.getContract();
        if (!contract) {
            throw new Error('Contract is not initialized');
        }
        const transaction = await contract.approve(spenderAddress, maxAmount);
        await transaction.wait();
        return true;
    }

    /**
     * @param userAddress - The address of the user
     * @param spenderAddress - The address of the spender
     * @returns A promise that resolves to the allowance amount
     */
    async getAllowance(userAddress: string, spenderAddress: string): Promise<bigint> {
        const contract = await this.getContract();
        if (!contract) {
            throw new Error('Contract is not initialized');
        }
        return await contract.allowance(userAddress, spenderAddress);
    }

    async approve(spenderAddress: string, amount: string): Promise<void> {
        const wei = parseUnits(amount, 'wei');
        const contract = await this.getContract();
        if (!contract) {
            throw new Error('Contract is not initialized');
        }
        const transaction = await contract.approve(spenderAddress, wei);
        await transaction.wait();
    }

    async checkAllowance(userAddress: string, address: string, amount: bigint): Promise<void> {
        const allowance = await this.getAllowance(userAddress, address);
        const amountBN = ethers.toBigInt(amount);
        if (amountBN <= allowance) {
            return;
        }
        const multiplier = ethers.toBigInt(10);
        await this.approve(address, (amountBN * multiplier).toString());
    }
}