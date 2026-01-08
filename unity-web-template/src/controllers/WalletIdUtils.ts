import {
    ChainId,
    getRpc,
    getSupportedChainId,
    getSupportedNetworkFromChainId,
    NetworkRpc,
    SupportedNetwork
} from "./RpcNetworkUtils.ts";
import {ethers} from "ethers";

export type WID = string; // Wallet ID format: walletAddress-chainId

export type WalletId = {
    wallet: WID;
    chainId: ChainId;
    network: SupportedNetwork;
    rpc: NetworkRpc;
    fullId: string;
};

const WALLET_LENGTH = 42; // Length of Ethereum address

function saveWalletId(wallet: string, chainId: ChainId, isProd: boolean): WalletId {
    const supportedNetwork = getSupportedNetworkFromChainId(chainId.dec);
    if (!supportedNetwork) {
        throw new Error('Unsupported chain ID');
    }
    wallet = wallet.toLowerCase();
    const walletId = `${wallet}-${chainId.dec}`;
    const url = new URL(window.location.href);
    url.searchParams.set('wid', walletId);
    window.history.replaceState({}, '', url);

    return {
        wallet: wallet,
        chainId: chainId,
        network: supportedNetwork,
        rpc: getRpc(supportedNetwork, isProd)!,
        fullId: walletId
    };
}

function getWalletId(isProd: boolean): WalletId | undefined {
    const url = new URL(window.location.href);
    const walletId = url.searchParams.get('wid');
    if (walletId) {
        const [wallet, chainIdDec] = walletId.split('-');
        if (!wallet || wallet.length !== WALLET_LENGTH) {
            return undefined;
        }
        if (!ethers.isAddress(wallet)) {
            return undefined;
        }
        const chainId = parseInt(chainIdDec);
        if (!chainId || isNaN(chainId)) {
            return undefined;
        }
        const supportedNetwork = getSupportedNetworkFromChainId(chainId);
        if (!supportedNetwork) {
            return undefined;
        }
        const w = wallet.toLowerCase();
        const chain = getSupportedChainId(supportedNetwork, isProd)!;
        const rpc = getRpc(supportedNetwork, isProd)!;
        return {
            wallet: w,
            chainId: chain,
            network: supportedNetwork,
            rpc: rpc,
            fullId: `${w}-${chainId}`
        };
    }
    return undefined;
}

function removeWalletId() {
    const url = new URL(window.location.href);
    url.searchParams.delete('wid');
    window.history.replaceState({}, '', url);
}

export const WalletIdUtils = {
    saveWalletId,
    getWalletId,
    removeWalletId
};