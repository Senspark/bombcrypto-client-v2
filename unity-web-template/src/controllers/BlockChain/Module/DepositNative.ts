import { Contract, JsonRpcProvider, formatUnits } from 'ethers';
import * as ContractUtils from './Utils/ContractUtils.js';
import { getDoubleGasFeeOptionV2, waitForReceipt } from "./Utils/NetworkUtils.js";
import { RpcService } from "../RpcService.ts";
import { INativeChain } from "../BlockChainData.ts";
import { BridgeTxResult } from "./DepositBridge.ts";

// The DepositNative vault (native BNB / POL). Same-chain only — BNB on BSC, POL on
// Polygon — so `chain` is just the login chain. deposit() is payable: the amount rides
// in msg.value, no token and no approve. withdraw relays a server signature and the
// contract self-computes amount = allowedCumulative − withdrawn[user], transferring
// FROM itself TO msg.sender. Reads go through a per-chain RPC provider (never the browser
// provider — no wallet switch); writes go through the browser signer, which is already on
// this chain because native is never cross-chain. Wei crosses in/out as an exact decimal
// string, never a JS number.
export default class DepositNative {
    private readonly _nativeChains: { [chain: string]: INativeChain };
    private readonly _abi: JSON;
    private readonly _rpcService: RpcService;

    constructor(nativeChains: { [chain: string]: INativeChain }, abi: JSON, rpcService: RpcService) {
        this._nativeChains = nativeChains;
        this._abi = abi;
        this._rpcService = rpcService;
    }

    private chainCfg(chain: string): INativeChain {
        const cfg = this._nativeChains[(chain ?? "").toUpperCase()];
        if (!cfg) throw new Error(`Unknown native chain '${chain}' (allowed: BSC, POLYGON)`);
        if (!cfg.nativeAddress) throw new Error(`DepositNative not deployed on '${chain}'`);
        return cfg;
    }

    private provider(chain: string): JsonRpcProvider {
        const cfg = this.chainCfg(chain);
        return new JsonRpcProvider(this._rpcService.getRpc(cfg.chainId));
    }

    private readContract(chain: string): Contract {
        return new Contract(this.chainCfg(chain).nativeAddress, JSON.stringify(this._abi), this.provider(chain));
    }

    private async writeContract(chain: string): Promise<Contract> {
        const contract = await ContractUtils.createWriteContract(this.chainCfg(chain).nativeAddress, this._abi);
        if (!contract) throw new Error('Can not create native contract');
        return contract;
    }

    async getDeposited(chain: string, user: string): Promise<string> {
        const value = await this.readContract(chain).deposited(user);
        return value.toString();
    }

    async getWithdrawn(chain: string, user: string): Promise<string> {
        const value = await this.readContract(chain).withdrawn(user);
        return value.toString();
    }

    async getDepositEnabled(chain: string): Promise<boolean> {
        return await this.readContract(chain).depositEnabled();
    }

    async getWithdrawEnabled(chain: string): Promise<boolean> {
        return await this.readContract(chain).withdrawEnabled();
    }

    // The user's native coin balance in their wallet, formatted to coin units — a display-only
    // "available" cap. The deposit amount itself is relayed as exact wei.
    async getWalletBalance(chain: string, user: string): Promise<number> {
        const bal: bigint = await this.provider(chain).getBalance(user);
        return Number(formatUnits(bal, 18));
    }

    // payable deposit — amount is msg.value, no token, no approve.
    async deposit(chain: string, amountWei: string): Promise<BridgeTxResult> {
        try {
            const value = BigInt(amountWei);
            const contract = await this.writeContract(chain);
            const estimateGas = await contract.deposit.estimateGas({ value });
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.deposit({ ...options, value });
            const txHash = transaction.hash ?? "";
            await waitForReceipt(transaction);
            return { success: true, txHash, net: 0 };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "", net: 0 };
        }
    }

    // withdraw(allowedCumulative, deadline, signature): the contract self-computes
    // amount = allowedCumulative − withdrawn[user] and pays it to msg.sender. No approve.
    async withdraw(chain: string, allowedCumulativeWei: string, deadline: string,
                   signature: string): Promise<BridgeTxResult> {
        try {
            const allowedCumulative = BigInt(allowedCumulativeWei);
            const deadlineSec = BigInt(deadline);
            const contract = await this.writeContract(chain);
            const estimateGas = await contract.withdraw.estimateGas(allowedCumulative, deadlineSec, signature);
            const options = await getDoubleGasFeeOptionV2(estimateGas);
            const transaction = await contract.withdraw(allowedCumulative, deadlineSec, signature, options);
            const txHash = transaction.hash ?? "";
            const receipt = await waitForReceipt(transaction);
            const net = this.readNetFromReceipt(contract, receipt);
            return { success: true, txHash, net };
        } catch (ex) {
            console.error(`exception ${ex}`);
            return { success: false, txHash: "", net: 0 };
        }
    }

    // Native paid out (no fee), read from the on-chain NativeWithdrawn event. Native is 18-decimals.
    private readNetFromReceipt(contract: Contract, receipt: unknown): number {
        const logs = (receipt as { logs?: Array<{ topics: readonly string[]; data: string }> })?.logs;
        if (!logs) return 0;
        for (const log of logs) {
            try {
                const parsed = contract.interface.parseLog(log);
                if (parsed && parsed.name === "NativeWithdrawn") {
                    return Number(formatUnits(parsed.args.amount, 18));
                }
            } catch {
                // log from another contract — skip
            }
        }
        return 0;
    }
}
