import {JsonRpcProvider} from 'ethers';
import {EnvConfig} from '../../configs/EnvConfig.ts';
import Logger from '../Logger.ts';

const TAG = '[RPC]';

// localStorage keys for the fetched/cached RPC lists
const STORAGE_KEY_BSC = '_rpc_list_bsc';
const STORAGE_KEY_POLYGON = '_rpc_list_polygon';

// Hardcoded fallback RPC lists (never persisted to localStorage)
const HARDCODE_BSC_MAINNET: string[] = [
    'https://bsc-dataseed.binance.org/',
    'https://bsc-dataseed1.binance.org/',
    'https://bsc-dataseed2.binance.org/',
    'https://bsc-dataseed3.binance.org/',
    'https://bsc-dataseed4.binance.org/',
];

const HARDCODE_POLYGON_MAINNET: string[] = [
    'https://polygon.api.onfinality.io/public',
    'https://polygon.rpc.subquery.network/public',
    'https://polygon.lava.build',
    'https://poly.api.pocket.network/',
    'https://rpc-mainnet.matic.quiknode.pro'
];


export class RpcService {
    private _bscRpcs: string[] = [];
    private _polygonRpcs: string[] = [];

    private readonly _rpcHost: string;

    constructor(
        private readonly _logger: Logger
    ) {
        this._rpcHost = EnvConfig.rpcHost();
    }

    /**
     * Fetches RPC lists from the API, falls back to localStorage then hardcoded lists.
     * Tests all RPCs concurrently and keeps only working ones per network.
     * Must be awaited before calling getRpc().
     */
    async initialize(): Promise<void> {
        this._logger.log(`${TAG} Initializing...`);

        const [bscRpcs, polygonRpcs] = await Promise.all([
            this._loadRpcList('bsc'),
            this._loadRpcList('polygon'),
        ]);

        const [bscWorking, polygonWorking] = await Promise.all([
            this._filterWorkingRpcs(bscRpcs),
            this._filterWorkingRpcs(polygonRpcs),
        ]);

        this._bscRpcs = bscWorking;
        this._polygonRpcs = polygonWorking;

        this._logger.log(`${TAG} BSC working RPCs (${this._bscRpcs.length}): ${this._bscRpcs.join(', ')}`);
        this._logger.log(`${TAG} Polygon working RPCs (${this._polygonRpcs.length}): ${this._polygonRpcs.join(', ')}`);
        this._logger.log(`${TAG} Initialized`);
    }

    /**
     * Returns a random verified working RPC URL for the given chainId.
     * Falls back to the first hardcoded entry if initialization failed.
     */
    getRpc(chainId: number): string {
        switch (chainId) {
            case 56:   // BSC mainnet
            case 97:   // BSC testnet
                return this._pickRandom(this._bscRpcs, HARDCODE_BSC_MAINNET);
            case 137:  // Polygon mainnet
            case 80002: // Polygon testnet (Amoy)
                return this._pickRandom(this._polygonRpcs, HARDCODE_POLYGON_MAINNET);
            default:
                this._logger.error(`${TAG} Unknown chainId: ${chainId}`);
                return '';
        }
    }

    /**
     * Returns a random element from the working list.
     * If the working list is empty, falls back to the first entry of the fallback list.
     */
    private _pickRandom(workingList: string[], fallback: string[]): string {
        if (workingList.length > 0) {
            return workingList[Math.floor(Math.random() * workingList.length)];
        }
        this._logger.error(`${TAG} Working RPC list is empty, using random hardcoded fallback`);
        return fallback[Math.floor(Math.random() * fallback.length)] ?? '';
    }


    /**
     * For Testing each rpc
     * @param workingList
     * @param fallback
     * @param index
     * @private
     */
    // private _pickAt(workingList: string[], fallback: string[], index: number): string {
    //     if (workingList.length > 0) {
    //         if(index <= workingList.length) {
    //             return workingList[index];
    //         }
    //         return workingList[0];
    //     }
    //     this._logger.error(`${TAG} Working RPC list is empty, using random hardcoded fallback`);
    //     return fallback[Math.floor(Math.random() * fallback.length)] ?? '';
    // }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    /**
     * Tries to load the RPC list for a network in this order:
     *  1. Fetch from remote API and save to localStorage
     *  2. Read from localStorage
     *  3. Use hardcoded fallback (NOT saved to localStorage)
     */
    private async _loadRpcList(network: 'bsc' | 'polygon'): Promise<string[]> {
        const storageKey = network === 'bsc' ? STORAGE_KEY_BSC : STORAGE_KEY_POLYGON;
        const endpoint = `${this._rpcHost}/${network}`;
        console.log(`Fetching RPC list for ${endpoint}`);

        // 1. Try remote API
        try {
            const response = await fetch(endpoint);
            if (response.ok) {
                const data = await response.json() as string[];
                if (Array.isArray(data) && data.length > 0) {
                    // Remove previous item first
                    localStorage.removeItem(storageKey);
                    // Save new rpc list we just fetch from server
                    localStorage.setItem(storageKey, JSON.stringify(data));
                    this._logger.log(`${TAG} Fetched ${network} RPC list from API (${data.length} entries)`);
                    return data;
                }
            }
        } catch (e) {
            this._logger.error(`${TAG} Failed to fetch ${network} RPC list from API: ${e}`);
        }

        // 2. Try localStorage
        try {
            const stored = localStorage.getItem(storageKey);
            if (stored) {
                const data = JSON.parse(stored) as string[];
                if (Array.isArray(data) && data.length > 0) {
                    this._logger.log(`${TAG} Loaded ${network} RPC list from localStorage (${data.length} entries)`);
                    return data;
                }
            }
        } catch (e) {
            this._logger.error(`${TAG} Failed to read ${network} RPC list from localStorage: ${e}`);
        }

        // 3. Hardcoded fallback — intentionally NOT saved to localStorage
        const fallback = network === 'bsc' ? HARDCODE_BSC_MAINNET : HARDCODE_POLYGON_MAINNET;
        this._logger.log(`${TAG} Using hardcoded fallback list for ${network} (${fallback.length} entries)`);
        return fallback;
    }

    /**
     * Tests all RPCs in the list concurrently and returns only the ones that
     * respond successfully to an eth_blockNumber call.
     * If none work, returns an empty array (caller handles the fallback).
     */
    private async _filterWorkingRpcs(rpcs: string[]): Promise<string[]> {
        const unique = [...new Set(rpcs)];

        const results = await Promise.allSettled(
            unique.map(async (rpc) => {
                const provider = new JsonRpcProvider(rpc);
                await provider.getBlockNumber();
                return rpc;
            })
        );

        const working: string[] = [];
        for (let i = 0; i < results.length; i++) {
            const result = results[i];
            if (result.status === 'fulfilled') {
                this._logger.log(`${TAG} Verified working RPC: ${unique[i]}`);
                working.push(result.value);
            } else {
                this._logger.error(`${TAG} RPC not working [${unique[i]}]: ${result.reason}`);
            }
        }

        return working;
    }
}

