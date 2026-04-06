import * as Storage from "./Storage.js";
import { ethers } from "ethers";
import {sleep} from "../../../../utils/Time.ts";
import {TransactionReceipt, TransactionResponse} from "ethers";
import CoinToken from "../CoinToken.ts";

const MIN_CONFIRMATIONS = 6;
const GAS_LIMIT_MULTIPLIER = 2;

class NetworkData {
    chainId: string;
    chainName: string;
    nativeCurrency: { name: string; symbol: string; decimals: number };
    rpcUrls: string[];
    blockExplorerUrls: string[];

    constructor(chainId: string, chainName: string, currency: string, rpcUrl: string, scanUrl: string) {
        this.chainId = chainId;
        this.chainName = chainName;
        this.nativeCurrency = {
            name: currency,
            symbol: currency,
            decimals: 18
        };
        this.rpcUrls = [rpcUrl];
        this.blockExplorerUrls = [scanUrl];
    }
}

const NetworkMap = new Map<string, NetworkData>([
    ["56", new NetworkData('0x38', 'Binance Smart Chain', 'BNB', 'https://bsc-dataseed.binance.org/', 'https://bscscan.com')],
    ["97", new NetworkData('0x61', 'BSC Test Net', 'BNB', 'https://data-seed-prebsc-1-s1.binance.org:8545/', 'https://testnet.bscscan.com')],
    ["137", new NetworkData('0x89', 'Matic Mainnet', 'MATIC', 'https://polygon-rpc.com/', 'https://polygonscan.com/')],
    ["80002", new NetworkData('0x13882', 'Polygon Test Net', 'MATIC', 'https://rpc-amoy.polygon.technology/', 'https://amoy.polygonscan.com/')],
]);

async function sign(message: string, userAddress: string): Promise<{ ec: number; signature: string }> {
    await sleep(1500);
    const result = { ec: 1, signature: '' };
    try {
        const signer =  await Storage.getBrowserProvider().getSigner(userAddress);
        result.signature = await signer.signMessage(message);
        result.ec = 0;
    } catch (ex) {
        console.error(`exception ${ex}`);
    }
    return result;
}

function getNetwork(chainId: bigint): NetworkData {
    if (!NetworkMap.has(chainId.toString())) {
        throw new Error('Invalid chain id');
    }
    return NetworkMap.get(chainId.toString())!;
}

// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-expect-error
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function waitForBlock(block: number): Promise<void> {
    while (true) {
        const provider = Storage.getBrowserProvider();
        const currentBlock = await provider.getBlockNumber();
        if (currentBlock >= block) {
            return;
        }
        await sleep(2000);
    }
}

function getDoubleGasFeeOption(estimateGas: bigint): { gasLimit: bigint } {
    const gasMultiplier = ethers.toBigInt(GAS_LIMIT_MULTIPLIER);
    return { gasLimit: estimateGas * gasMultiplier};
}

/**
 * Returns gas options with doubled gas limit.
 * On Polygon (mainnet or Amoy), also applies a 30% premium on EIP-1559 fees
 * to improve transaction inclusion speed during congestion.
 */
async function getDoubleGasFeeOptionV2(
    estimateGas: bigint
): Promise<{
    gasLimit: bigint;
    maxFeePerGas?: bigint;
    maxPriorityFeePerGas?: bigint;
}> {
    const gasMultiplier = ethers.toBigInt(GAS_LIMIT_MULTIPLIER);
    const gasLimit = estimateGas * gasMultiplier;

    // Only attempt EIP-1559 fee enhancement on Polygon networks
    const GAS_FEE_PREMIUM = 130n; // 30% premium
    const POLYGON_CHAIN_IDS = [137, 80002]; // Polygon Mainnet & Amoy Testnet

    try {
        const provider = Storage.getBrowserProvider();
        if (!provider) {
            return { gasLimit };
        }

        const network = await provider.getNetwork();
        const chainId = Number(network.chainId);

        if (!POLYGON_CHAIN_IDS.includes(chainId)) {
            return { gasLimit }; // Use simple gasLimit on other chains
        }

        // Polygon-specific: Apply 30% fee premium for better inclusion
        const feeData = await provider.getFeeData();

        if (feeData.maxFeePerGas && feeData.maxPriorityFeePerGas) {
            const maxPriorityFeePerGas = (feeData.maxPriorityFeePerGas * GAS_FEE_PREMIUM) / 100n;
            const baseFee = feeData.maxFeePerGas - feeData.maxPriorityFeePerGas;
            const maxFeePerGas = baseFee + maxPriorityFeePerGas;

            return {
                gasLimit,
                maxFeePerGas,
                maxPriorityFeePerGas,
            };
        }
    } catch (error) {
        console.warn("Failed to fetch enhanced EIP-1559 fees for Polygon. Falling back to gasLimit only.", error);
    }

    // Fallback for all other cases (non-Polygon, provider error, missing fee data, etc.)
    return { gasLimit };
}

async function waitForReceipt(transaction: TransactionResponse): Promise<TransactionReceipt | null> {
    return await transaction.wait(MIN_CONFIRMATIONS);
}

function getAllNetworkRpc(): { [key: number]: string } {
    const rpc: { [key: string]: string } = {};
    NetworkMap.forEach((value, key) => {
        rpc[key] = value.rpcUrls[0];
    });
    return rpc;
}
function getCoinTokenByBuyHeroCategory(category: number): CoinToken {
    let coinToken: CoinToken;
    if (category === 0) {
        coinToken = Storage.getToken('bcoin');
    } else if (category === 1 || category === 2) {
        coinToken = Storage.getToken('sen');
    } else {
        throw new Error(`Invalid category ${category}`);
    }
    return coinToken;
}

export {
    sign,
    getDoubleGasFeeOption,
    getDoubleGasFeeOptionV2,
    waitForReceipt,
    getAllNetworkRpc,
    getCoinTokenByBuyHeroCategory,
    getNetwork
};