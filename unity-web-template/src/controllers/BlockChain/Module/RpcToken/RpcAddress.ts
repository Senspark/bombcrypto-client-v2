// Một network sẽ có nhiều rpc.
// Ban đầu request sẽ random index các rpc. Xài cái này ko được thì xài cái tiếp theo

const RPC_BNB = '_rpc_url_bnb';
const RPC_BNB_TESTNET = '_rpc_url_bnb_testnet';

const RPC_POLYGON = '_rpc_url_polygon';
const RPC_POLYGON_TESTNET = '_rpc_url_polygon_testnet';

/**
 * @param {number} chainId
 * @return {string[]}
 */
export default function getAllRpc(chainId: number): string[] {
    switch (chainId) {
        case 56: // BNB - main net
            return [
                'https://bsc-dataseed.binance.org/',
                'https://bsc-dataseed1.binance.org/',
                'https://bsc-dataseed2.binance.org/',
                'https://bsc-dataseed3.binance.org/',
                'https://bsc-dataseed4.binance.org/',
            ];
        case 97: // BNB - testnet
            return [
                'https://data-seed-prebsc-1-s1.binance.org:8545/',
                'https://data-seed-prebsc-2-s1.binance.org:8545/',
                'https://data-seed-prebsc-1-s3.binance.org:8545/',
                'https://data-seed-prebsc-2-s3.binance.org:8545/',
            ];
        case 137: // Polygon main net
            return [
                'https://polygon-rpc.com/',
            ];
        case 80002: // Polygon test net
            return [
                'https://rpc-amoy.polygon.technology/',
            ];
        default:
            return [];
    }
}

// Chưa encrypt data
/**
 * @param {number} chainId
 * @return {string[]}
 */
export function getCustomRpc(chainId: number): string[] {
    let rpc: string | null;
    switch (chainId) {
        case 56: // BNB - main net
            rpc = localStorage.getItem(RPC_BNB);
            break;
        case 97: // BNB - testnet
            rpc = localStorage.getItem(RPC_BNB_TESTNET);
            break;
        case 137: // Polygon main net
            rpc = localStorage.getItem(RPC_POLYGON);
            break;
        case 80002: // Polygon test net
            rpc = localStorage.getItem(RPC_POLYGON_TESTNET);
            break;
        default:
            rpc = null;
            break;
    }
    if (rpc) {
        rpc = rpc.replace(/^"|"$/g, ''); // remove quotes
        return [rpc];
    }
    return [];
}