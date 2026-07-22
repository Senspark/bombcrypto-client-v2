import GeneralContract from './Utils/GeneralContract.js';
import {getDoubleGasFeeOptionV2} from "./Utils/NetworkUtils";
import {Contract, parseEther} from "ethers";
import BHeroToken from "./BHero.ts";

export type ClaimAndProcessResult = {
    txHash: string;
    processResult: { result: boolean; fusionFailAmount: number; fusionSuccessHeroIds: number[] } | null;
};

export default class ClaimManager extends GeneralContract {
    private readonly _bhero: BHeroToken;

    constructor(address: string, abi: any, bhero: BHeroToken) {
        super(address, abi);
        this._bhero = bhero;
    }

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
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.claimTokens(tokenType, amountValue, nonce, details, signature, options);
            const receipt = await transaction.wait(waitConfirmations);
            return receipt?.hash.toString();
        } catch (ex) {
            console.error(`exception ${ex}`);
            return "";
        }
    }

    // Combined claim + post-mint orchestration. For Hero-token claims, ClaimManage.claimTokens
    // creates pending mint requests on BHero that MUST be processed by BHero.processTokenRequests
    // in the same flow — otherwise the heroes are stranded on-chain. Putting the orchestration
    // here keeps blockchain logic in TS so future fixes don't require a Unity/WebGL rebuild.
    async claimTokensAndProcess(
        tokenType: string,
        amount: number,
        nonce: number,
        details: string,
        signature: string,
        formatType: string,
        waitConfirmations: number,
        walletAddress: string,
    ): Promise<ClaimAndProcessResult> {
        const txHash = await this.claimTokens(
            tokenType, amount, nonce, details, signature, formatType, waitConfirmations,
        );
        if (!txHash) {
            return { txHash: "", processResult: null };
        }

        // tokenType is the int category index (e.g. 1 = Hero on BSC testnet) understood by
        // ClaimManage. Resolve it to the underlying token contract address via the public
        // `tokenContracts` mapping, then compare to BHero — only Hero claims need processing.
        let isHero = false;
        try {
            const contract = await this.getContract();
            const tokenAddress: string = await contract.tokenContracts(tokenType);
            isHero = tokenAddress?.toLowerCase() === this._bhero._address?.toLowerCase();
        } catch (ex) {
            console.error(`tokenContracts lookup failed: ${ex}`);
        }
        if (!isHero) {
            return { txHash, processResult: null };
        }

        const processResult = await this._bhero.processTokenRequestsV2(walletAddress);
        return { txHash, processResult };
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
    //         const options = await getDoubleGasFeeOptionV2(estimateGas);
    //         const transaction = await contract.buyHeroUseVoucher(tokenPay, voucherType, heroQuantity, amount, nonce, signature, options);
    //         await transaction.wait();
    //         return Message.Info();
    //     } catch (ex) {
    //         return Message.Error(ex.message);
    //     }
    // }
}