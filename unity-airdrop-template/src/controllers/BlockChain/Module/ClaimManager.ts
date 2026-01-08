import GeneralContract from './Utils/GeneralContract.js';
import {getDoubleGasFeeOption} from "./Utils/NetworkUtils.js";
import {Contract, parseEther} from "ethers";

export default class ClaimManager extends GeneralContract {
    async claimTokens(
        tokenType: string,
        amount: number,
        nonce: number,
        details: string,
        signature: string,
        formatType: string,
        waitConfirmations: number
    ): Promise<string> {
        try {
            const contract: Contract = await this.getContract();
            let amountValue: string | number = amount;
            if (formatType) {
                amountValue = parseEther(amount.toString()).toString();
            }
            const estimateGas = await contract.claimTokens.estimateGas(tokenType, amountValue, nonce, details, signature);
            const options = getDoubleGasFeeOption(estimateGas);
            const transaction = await contract.claimTokens(tokenType, amountValue, nonce, details, signature, options);
            const receipt = await transaction.wait(waitConfirmations);
            return receipt?.hash.toString();
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "";
        }
    }

    async userCanUseVoucher(voucherType: string, userAddress: string): Promise<boolean> {
        const contract = await this.getContract();
        return contract.userCanUseVoucher(voucherType, userAddress);
    }

    //FIXME: Ko còn dùng trong game
    // async buyHeroUseVoucher(
    //     userAddress: string,
    //     tokenPay: string,
    //     voucherType: string,
    //     heroQuantity: number,
    //     amount: number,
    //     nonce: number,
    //     signature: string
    // ): Promise<Message.Message> {
    //     try {
    //         const token = getData(tokenPay);
    //         await token.checkAllowance(userAddress, this._address, amount);
    //         const contract: Contract = this.getContract();
    //         const estimateGas = await contract.estimateGas.buyHeroUseVoucher(tokenPay, voucherType, heroQuantity, amount, nonce, signature);
    //         const options = getDoubleGasFeeOption(estimateGas);
    //         const transaction = await contract.buyHeroUseVoucher(tokenPay, voucherType, heroQuantity, amount, nonce, signature, options);
    //         await transaction.wait();
    //         return Message.Info();
    //     } catch (ex) {
    //         return Message.Error(ex.message);
    //     }
    // }
}