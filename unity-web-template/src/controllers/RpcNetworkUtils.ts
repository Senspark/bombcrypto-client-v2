export type NetworkRpc = {
    chainId: number,
    chainIdHex: string,
    chainName: string,
    rpcUrl: string,
    currencySymbol: string,
    decimals: number,
    blockExplorerUrl: string
};

export type SupportedNetwork = 'bsc' | 'polygon' | 'ronin' | 'base' | 'vic';
export type ChainId = {
    dec: number,
    hex: string
};
type ChainIdMap = Map<SupportedNetwork, ChainId>;

export function getRpc(network: SupportedNetwork, isProduction: boolean): NetworkRpc | undefined {
    if (isProduction) {
        switch (network) {
            case 'bsc':
                return BNB_MAINNET_RPC;
            case 'polygon':
                return POLYGON_MAINNET_RPC;
            case 'ronin':
                return RONIN_MAINNET_RPC;
            case 'base':
                return BASE_MAINNET_RPC;
            case 'vic':
                return VIC_MAINNET_RPC;
            default:
                return undefined;
        }
    } else {
        switch (network) {
            case 'bsc':
                return BNB_TESTNET_RPC;
            case 'polygon':
                return POLYGON_TESTNET_RPC;
            case 'ronin':
                return RONIN_TESTNET_RPC;
            case 'base':
                return BASE_TESTNET_RPC;
            case 'vic':
                return VIC_TESTNET_RPC;
            default:
                return undefined;
        }
    }
}

export function getSupportedChainId(network: SupportedNetwork, isProduction: boolean): ChainId | undefined {
    if (isProduction) {
        return ChainIdsProd.get(network);
    } else {
        return ChainIdsTest.get(network);
    }
}

export function getSupportedNetworkFromChainId(chainId: number | undefined | null, isProduction: boolean | undefined = undefined): SupportedNetwork | undefined {
    if (!chainId) {
        return undefined;
    }
    const findFromMap = (chainIds: ChainIdMap) => {
        for (const [network, id] of chainIds.entries()) {
            if (id.dec === chainId) {
                return network;
            }
        }
        return undefined;
    }

    if (isProduction != undefined) {
        if (isProduction) {
            return findFromMap(ChainIdsProd);
        } else {
            return findFromMap(ChainIdsTest);
        }
    }

    const prod = findFromMap(ChainIdsProd);
    if (prod) {
        return prod;
    }
    const test = findFromMap(ChainIdsTest);
    if (test) {
        return test;
    }
    return undefined;
}

export const BNB_MAINNET_RPC: NetworkRpc = {
    chainId: 56,
    chainIdHex: '0x38',
    chainName: 'Binance Smart Chain Mainnet',
    rpcUrl: 'https://bsc-dataseed.binance.org/',
    currencySymbol: 'BNB',
    decimals: 18,
    blockExplorerUrl: 'https://bscscan.com/'
}

export const BNB_TESTNET_RPC: NetworkRpc = {
    chainId: 97,
    chainIdHex: '0x61',
    chainName: 'Binance Smart Chain Testnet',
    rpcUrl: 'https://data-seed-prebsc-1-s1.binance.org:8545/',
    currencySymbol: 'BNB',
    decimals: 18,
    blockExplorerUrl: 'https://testnet.bscscan.com/'
}

export const POLYGON_MAINNET_RPC: NetworkRpc = {
    chainId: 137,
    chainIdHex: '0x89',
    chainName: 'Polygon Mainnet',
    rpcUrl: 'https://polygon-mainnet.infura.io/',
    currencySymbol: 'POL',
    decimals: 18,
    blockExplorerUrl: 'https://polygonscan.com/'
}

export const POLYGON_TESTNET_RPC: NetworkRpc = {
    chainId: 80002,
    chainIdHex: '0x13882',
    chainName: 'Polygon Amoy Testnet',
    rpcUrl: 'https://rpc-amoy.polygon.technology/',
    currencySymbol: 'POL',
    decimals: 18,
    blockExplorerUrl: 'https://amoy.polygonscan.com/'
}

export const RONIN_MAINNET_RPC: NetworkRpc = {
    chainId: 2020,
    chainIdHex: '0x7e4',
    chainName: 'Ronin Mainnet',
    rpcUrl: 'https://api.roninchain.com/rpc',
    currencySymbol: 'RON',
    decimals: 18,
    blockExplorerUrl: 'https://app.roninchain.com/'
}

export const RONIN_TESTNET_RPC: NetworkRpc = {
    chainId: 2021,
    chainIdHex: '0x7e5',
    chainName: 'Ronin Saigon Testnet',
    rpcUrl: 'https://saigon-testnet.roninchain.com/rpc',
    currencySymbol: 'RON',
    decimals: 18,
    blockExplorerUrl: 'https://saigon-app.roninchain.com/'
}

export const BASE_MAINNET_RPC: NetworkRpc = {
    chainId: 8453,
    chainIdHex: '0x2105',
    chainName: 'Base Mainnet',
    rpcUrl: 'https://mainnet.base.org/',
    currencySymbol: 'ETH',
    decimals: 18,
    blockExplorerUrl: 'https://basescan.org/'
}

export const BASE_TESTNET_RPC: NetworkRpc = {
    chainId: 84532,
    chainIdHex: '0x14a34',
    chainName: 'Base Sepolia',
    rpcUrl: 'https://sepolia.base.org/',
    currencySymbol: 'ETH',
    decimals: 18,
    blockExplorerUrl: 'https://sepolia.basescan.org/'
}

export const VIC_MAINNET_RPC: NetworkRpc = {
    chainId: 88,
    chainIdHex: '0x58',
    chainName: 'Viction',
    rpcUrl: 'https://rpc.viction.xyz',
    currencySymbol: 'VIC',
    decimals: 18,
    blockExplorerUrl: 'https://vicscan.xyz'
};

export const VIC_TESTNET_RPC: NetworkRpc = {
    chainId: 89,
    chainIdHex: '0x59',
    chainName: 'Viction Testnet',
    rpcUrl: 'https://rpc-testnet.viction.xyz',
    currencySymbol: 'VIC',
    decimals: 18,
    blockExplorerUrl: 'https://testnet.vicscan.xyz'
};

const ChainIdsProd: ChainIdMap = new Map<SupportedNetwork, ChainId>([
    ['bsc', getChainId(BNB_MAINNET_RPC)],
    ['polygon', getChainId(POLYGON_MAINNET_RPC)],
    ['ronin', getChainId(RONIN_MAINNET_RPC)],
    ['base', getChainId(BASE_MAINNET_RPC)],
    // ['vic', getChainId(VIC_MAINNET_RPC)]
]);
const ChainIdsTest: ChainIdMap = new Map<SupportedNetwork, ChainId>([
    ['bsc', getChainId(BNB_TESTNET_RPC)],
    ['polygon', getChainId(POLYGON_TESTNET_RPC)],
    ['ronin', getChainId(RONIN_TESTNET_RPC)],
    ['base', getChainId(BASE_TESTNET_RPC)],
    // ['vic', getChainId(VIC_TESTNET_RPC)]
]);

function getChainId(from: NetworkRpc): ChainId {
    return {
        dec: from.chainId,
        hex: from.chainIdHex
    };
}