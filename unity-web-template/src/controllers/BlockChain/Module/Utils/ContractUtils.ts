import * as Storage from "./Storage.js";
import {
    Contract,
    ContractRunner,
    ethers,
    isError,
    JsonRpcProvider,
    TransactionRequest,
    TransactionResponse
} from "ethers";
import {dropReadProvider, getReadProvider} from "./ReadProvider.ts";

// Reads and receipts use an RPC pinned to one chain; only sending a transaction
// touches the wallet, which is switched to that chain just for the signature.
class TabRunner implements ContractRunner {
    constructor(private readonly _chainId: number) {
    }

    get provider(): JsonRpcProvider {
        return getReadProvider(this._chainId);
    }

    call(tx: TransactionRequest): Promise<string> {
        return this.read(provider => provider.call(this.withFrom(tx)));
    }

    estimateGas(tx: TransactionRequest): Promise<bigint> {
        return this.read(provider => provider.estimateGas(this.withFrom(tx)));
    }

    resolveName(name: string): Promise<null | string> {
        return this.provider.resolveName(name);
    }

    sendTransaction(tx: TransactionRequest): Promise<TransactionResponse> {
        const gate = Storage.getWalletGate();
        if (!gate) {
            throw new Error('Wallet is not ready');
        }
        return gate.run(this._chainId, async (provider) => {
            const signer = await provider.getSigner(gate.getAddress() ?? undefined);
            // chainId makes the wallet reject the tx if it is on another network
            return signer.sendTransaction({...tx, chainId: this._chainId});
        });
    }

    private withFrom(tx: TransactionRequest): TransactionRequest {
        const from = tx.from ?? Storage.getWalletGate()?.getAddress();
        return from ? {...tx, from} : tx;
    }

    private async read<T>(fn: (provider: JsonRpcProvider) => Promise<T>): Promise<T> {
        try {
            return await fn(this.provider);
        } catch (e) {
            if (!isError(e, 'SERVER_ERROR') && !isError(e, 'NETWORK_ERROR') && !isError(e, 'TIMEOUT')) {
                throw e;
            }
            dropReadProvider(this._chainId);
            return await fn(this.provider);
        }
    }
}

/**
 *
 * @param {string} address
 * @param {string} abi
 * @returns {Contract | null}
 */
function createReadContract(address: string, abi: JSON): Contract | null {
    if (address) {
        const provider = getReadProvider(Storage.getTabChainId());
        return new ethers.Contract(address, JSON.stringify(abi), provider);
    }
    console.warn('Empty abi and address');
    return null;
}

/**
 *
 * @param {string} address
 * @param {string} abi
 * @param {number} chainId - defaults to the chain this tab logged in with
 * @returns {Contract | null}
 */
async function createWriteContract(address: string, abi: JSON, chainId?: number): Promise<Contract | null> {
    if (address) {
        const runner = new TabRunner(chainId ?? Storage.getTabChainId());
        return new ethers.Contract(address, JSON.stringify(abi), runner);
    }
    console.warn('Empty abi and address');
    return null;
}

export {
    createReadContract,
    createWriteContract
}
