import GeneralContract from './Utils/GeneralContract.js';
import { getDoubleGasFeeOptionV2, waitForReceipt } from "./Utils/NetworkUtils.js";
import CoinToken from "./CoinToken.ts";

export interface BridgeTxResult {
    success: boolean;
    // empty when success === false (no tx broadcast / receipt reverted)
    txHash: string;
}

// Cross-chain DepositBridge gateway for the real MetaMask (WebGL) path. Mirrors
// editor-chain-proxy/src/game/bridge.ts: deposit auto-approves MaxUint256 (so the
// user signs the ERC20 approve at most once per token, never again), withdraw needs
// NO approve (the contract safeTransfers contract→user). Wei crosses in/out as an
// exact decimal string — never a JS number (§I of the master plan).
export default class DepositBridge extends GeneralContract {
    private readonly _bcoinToken: CoinToken;
    private readonly _sensparkToken: CoinToken;

    constructor(address: string, abi: JSON, bcoinToken: CoinToken, sensparkToken: CoinToken) {
        super(address, abi);
        this._bcoinToken = bcoinToken;
        this._sensparkToken = sensparkToken;
    }

    private tokenOf(symbol: string): CoinToken {
        const s = (symbol ?? "").toUpperCase();
        if (s === "BCOIN") return this._bcoinToken;
        if (s === "SEN") return this._sensparkToken;
        throw new Error(`Unknown bridge token '${symbol}' (allowed: BCOIN, SEN)`);
    }

    async getDeposited(walletAddress: string, symbol: string): Promise<string> {
        const contract = await this.getContract();
        const value = await contract.deposited(walletAddress, this.tokenOf(symbol)._address);
        return value.toString();
    }

    async getWithdrawn(walletAddress: string, symbol: string): Promise<string> {
        const contract = await this.getContract();
        const value = await contract.withdrawn(walletAddress, this.tokenOf(symbol)._address);
        return value.toString();
    }

    async deposit(walletAddress: string, symbol: string, amountWei: string): Promise<BridgeTxResult> {
        try {
            const token = this.tokenOf(symbol);
            const amount = BigInt(amountWei);
            // approveMaximum → MaxUint256 when current allowance is short, so future
            // deposits skip the approve prompt (and its gas) entirely.
            await token.approveMaximum(walletAddress, this._address, amount);

            const contract = await this.getContract();
            const estimateGas = await contract.deposit.estimateGas(token._address, amount);
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.deposit(token._address, amount, options);
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "" };
        }
    }

    async withdraw(symbol: string, grossWei: string, beforeWei: string,
                   signature: string): Promise<BridgeTxResult> {
        try {
            const tokenAddress = this.tokenOf(symbol)._address;
            const gross = BigInt(grossWei);
            const before = BigInt(beforeWei);

            const contract = await this.getContract();
            const estimateGas = await contract.withdraw.estimateGas(tokenAddress, gross, before, signature);
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.withdraw(tokenAddress, gross, before, signature, options);
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "" };
        }
    }
}
