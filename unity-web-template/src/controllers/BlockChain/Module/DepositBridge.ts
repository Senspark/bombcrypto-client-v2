import { Contract, JsonRpcProvider, MaxUint256, formatUnits } from 'ethers';
import * as ContractUtils from './Utils/ContractUtils.js';
import { getDoubleGasFeeOptionV2, waitForReceipt } from "./Utils/NetworkUtils.js";
import { RpcService } from "../RpcService.ts";
import { IBridgeChain } from "../BlockChainData.ts";

export interface BridgeTxResult {
    success: boolean;
    // empty when success === false (no tx broadcast / receipt reverted)
    txHash: string;
    // net token amount received (already formatted from wei), read from the on-chain
    // Withdraw event. 0 for deposit / failure.
    net: number;
}

// Cross-chain DepositBridge gateway. Every operation targets a chain that may
// differ from the wallet's login chain, so it is resolved per call:
//   - reads (deposited/withdrawn/enabled) go through an RPC-per-chain provider
//     (RpcService.getRpc), never the browser provider — no wallet switch needed.
//   - writes (deposit/withdraw) go through the browser signer, which the caller
//     has already switched onto the target chain (see UnityCommunicator).
// deposit auto-approves MaxUint256 (one approve per token, ever); withdraw needs
// NO approve — the contract safeTransfers contract→user, self-computing
// amount = otherDeposited − withdrawn. Wei crosses in/out as an exact decimal
// string, never a JS number.
export default class DepositBridge {
    private readonly _bridgeChains: { [chain: string]: IBridgeChain };
    private readonly _abi: JSON;
    private readonly _erc20Abi: JSON;
    private readonly _rpcService: RpcService;

    constructor(bridgeChains: { [chain: string]: IBridgeChain }, abi: JSON,
                erc20Abi: JSON, rpcService: RpcService) {
        this._bridgeChains = bridgeChains;
        this._abi = abi;
        this._erc20Abi = erc20Abi;
        this._rpcService = rpcService;
    }

    private chainCfg(chain: string): IBridgeChain {
        const cfg = this._bridgeChains[(chain ?? "").toUpperCase()];
        if (!cfg) throw new Error(`Unknown bridge chain '${chain}' (allowed: BSC, POLYGON)`);
        return cfg;
    }

    private tokenAddr(chain: string, symbol: string): string {
        const cfg = this.chainCfg(chain);
        const s = (symbol ?? "").toUpperCase();
        if (s === "BCOIN") return cfg.tokens.BCOIN;
        if (s === "SEN") return cfg.tokens.SEN;
        throw new Error(`Unknown bridge token '${symbol}' (allowed: BCOIN, SEN)`);
    }

    // Read-only contract on the target chain via a dedicated RPC — independent of
    // whatever chain the wallet is currently on.
    private readContract(chain: string): Contract {
        const cfg = this.chainCfg(chain);
        const rpc = this._rpcService.getRpc(cfg.chainId);
        const provider = new JsonRpcProvider(rpc);
        return new Contract(cfg.bridgeAddress, JSON.stringify(this._abi), provider);
    }

    // Write contract through the browser signer (wallet must already be on the
    // target chain — the caller switches it before invoking a write).
    private async writeContract(chain: string): Promise<Contract> {
        const cfg = this.chainCfg(chain);
        const contract = await ContractUtils.createWriteContract(cfg.bridgeAddress, this._abi);
        if (!contract) throw new Error('Can not create bridge contract');
        return contract;
    }

    async getDeposited(chain: string, walletAddress: string, symbol: string): Promise<string> {
        const value = await this.readContract(chain).deposited(walletAddress, this.tokenAddr(chain, symbol));
        return value.toString();
    }

    async getWithdrawn(chain: string, walletAddress: string, symbol: string): Promise<string> {
        const value = await this.readContract(chain).withdrawn(walletAddress, this.tokenAddr(chain, symbol));
        return value.toString();
    }

    async getDepositEnabled(chain: string): Promise<boolean> {
        return await this.readContract(chain).depositEnabled();
    }

    async getWithdrawEnabled(chain: string): Promise<boolean> {
        return await this.readContract(chain).withdrawEnabled();
    }

    async deposit(chain: string, walletAddress: string, symbol: string,
                  amountWei: string): Promise<BridgeTxResult> {
        try {
            const cfg = this.chainCfg(chain);
            const tokenAddress = this.tokenAddr(chain, symbol);
            const amount = BigInt(amountWei);

            await this.approveMaximum(walletAddress, tokenAddress, cfg.bridgeAddress, amount);

            const contract = await this.writeContract(chain);
            const estimateGas = await contract.deposit.estimateGas(tokenAddress, amount);
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.deposit(tokenAddress, amount, options);
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash, net: 0 };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "", net: 0 };
        }
    }

    // withdraw(token, otherDeposited, deadline, signature): the contract self-computes
    // amount = otherDeposited − withdrawn[user][token]. No user approve needed.
    async withdraw(chain: string, symbol: string, otherDepositedWei: string,
                   deadline: string, signature: string): Promise<BridgeTxResult> {
        try {
            const tokenAddress = this.tokenAddr(chain, symbol);
            const otherDeposited = BigInt(otherDepositedWei);
            const deadlineSec = BigInt(deadline);

            const contract = await this.writeContract(chain);
            const estimateGas = await contract.withdraw.estimateGas(
                tokenAddress, otherDeposited, deadlineSec, signature);
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.withdraw(
                tokenAddress, otherDeposited, deadlineSec, signature, options);
            const txHash = transaction.hash ?? "";
            const receipt = await waitForReceipt(transaction);
            const net = this.readNetFromReceipt(contract, receipt);
            return { success: true, txHash, net };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "", net: 0 };
        }
    }

    // Exact net received (after fee), read from the on-chain Withdraw event —
    // more accurate than a client-side fee estimate. Bridge tokens are 18-decimals.
    private readNetFromReceipt(contract: Contract, receipt: unknown): number {
        const logs = (receipt as { logs?: Array<{ topics: readonly string[]; data: string }> })?.logs;
        if (!logs) return 0;
        for (const log of logs) {
            try {
                const parsed = contract.interface.parseLog(log);
                if (parsed && parsed.name === "Withdraw") {
                    return Number(formatUnits(parsed.args.net, 18));
                }
            } catch {
                // log from another contract (e.g. ERC20 Transfer) — skip
            }
        }
        return 0;
    }

    // approve MaxUint256 on the target-chain ERC20 (browser signer) when the
    // current allowance is short, so future deposits skip the approve prompt.
    private async approveMaximum(walletAddress: string, tokenAddress: string,
                                 spender: string, amount: bigint): Promise<void> {
        const token = await ContractUtils.createWriteContract(tokenAddress, this._erc20Abi);
        if (!token) throw new Error('Can not create token contract');
        const allowance: bigint = await token.allowance(walletAddress, spender);
        if (allowance >= amount) return;
        const transaction = await token.approve(spender, MaxUint256);
        await transaction.wait();
    }
}
