import {EnvConfig} from '../../configs/EnvConfig.ts';
import Logger from '../Logger.ts';

const TAG = '[RPC]';

// Bounds how long a single RPC candidate/list-fetch can stall this (non-blocking) probe.
const RPC_TEST_TIMEOUT_MS = 5000;
const RPC_LIST_FETCH_TIMEOUT_MS = 5000;

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

const HARDCODE_BSC_TESTNET: string[] = [
    'https://bsc-testnet-dataseed.bnbchain.org',
    'https://bsc-testnet.bnbchain.org',
    'https://bsc-prebsc-dataseed.bnbchain.org',
];

const HARDCODE_POLYGON_MAINNET: string[] = [
    'https://polygon.api.onfinality.io/public',
    // 'https://polygon.rpc.subquery.network/public',
    'https://polygon.lava.build',
    'https://poly.api.pocket.network/',
    'https://rpc-mainnet.matic.quiknode.pro'
];

const HARDCODE_POLYGON_TESTNET: string[] = [
    'https://polygon-amoy.drpc.org',
];

export class RpcService {
    private _bscRpcs: string[] = [];
    private _polygonRpcs: string[] = [];

    // Everything that was offered for each network, working or not. Probing runs
    // in the background, so callers that need an URL before it finishes fall back
    // to these instead of to a single hardcoded entry.
    private _bscCandidates: string[] = [];
    private _polygonCandidates: string[] = [];

    private readonly _listeners: Array<() => void> = [];

    private readonly _rpcHost: string;
    private readonly _isProduction: boolean;

    constructor(
        private readonly _logger: Logger
    ) {
        this._rpcHost = EnvConfig.rpcHost();
        this._isProduction = EnvConfig.isProduction();
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

        this._bscCandidates = [...new Set(bscRpcs)];
        this._polygonCandidates = [...new Set(polygonRpcs)];

        const [bscWorking, polygonWorking] = await Promise.all([
            this._filterWorkingRpcs(this._bscCandidates),
            this._filterWorkingRpcs(this._polygonCandidates),
        ]);

        this._bscRpcs = bscWorking;
        this._polygonRpcs = polygonWorking;

        this._logger.log(`${TAG} BSC working RPCs (${this._bscRpcs.length}): ${this._bscRpcs.join(', ')}`);
        this._logger.log(`${TAG} Polygon working RPCs (${this._polygonRpcs.length}): ${this._polygonRpcs.join(', ')}`);
        this._logger.log(`${TAG} Initialized`);

        this._notifyUpdated();
    }

    /**
     * True when no RPC answered for this chain, so getRpc() is handing out an
     * unverified entry. The caller warns the player instead of leaving them
     * wondering why balances are empty and actions keep failing.
     */
    hasNoWorkingRpc(chainId: number): boolean {
        switch (chainId) {
            case 56:
            case 97:
                return this._bscRpcs.length === 0;
            case 137:
            case 80002:
                return this._polygonRpcs.length === 0;
            default:
                return false;
        }
    }

    /**
     * Every usable URL for this chain: the verified ones once probing finished,
     * otherwise the full candidate list, so the caller can walk it until one
     * answers instead of being stuck with a single unverified entry.
     */
    getRpcs(chainId: number): string[] {
        switch (chainId) {
            case 56:
            case 97:
                return this._pickList(this._bscRpcs, this._bscCandidates, chainId);
            case 137:
            case 80002:
                return this._pickList(this._polygonRpcs, this._polygonCandidates, chainId);
            default:
                this._logger.error(`${TAG} Unknown chainId: ${chainId}`);
                return [];
        }
    }

    private _pickList(working: string[], candidates: string[], chainId: number): string[] {
        if (working.length > 0) {
            return [...working];
        }
        if (candidates.length > 0) {
            return [...candidates];
        }
        return [...this._fallbackList(chainId)];
    }

    /**
     * Called whenever a probing round finishes and the working lists changed.
     * Consumers built before probing ended use it to swap in the verified URLs.
     */
    onRpcsUpdated(listener: () => void): void {
        this._listeners.push(listener);
    }

    private _notifyUpdated(): void {
        for (const listener of this._listeners) {
            try {
                listener();
            } catch (e) {
                this._logger.error(`${TAG} RPC update listener failed: ${e}`);
            }
        }
    }

    /**
     * Returns a random verified working RPC URL for the given chainId.
     * Falls back to the first hardcoded entry if initialization failed.
     */
    getRpc(chainId: number): string {
        switch (chainId) {
            case 56:   // BSC mainnet
            case 97:   // BSC testnet
                return this._pickRandom(this._bscRpcs, this._fallbackList(chainId));
            case 137:  // Polygon mainnet
            case 80002: // Polygon testnet (Amoy)
                return this._pickRandom(this._polygonRpcs, this._fallbackList(chainId));
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
     * The built-in list for a chain, used when nothing was verified yet.
     */
    private _fallbackList(chainId: number): string[] {
        switch (chainId) {
            case 56:
                return HARDCODE_BSC_MAINNET;
            case 97:
                return HARDCODE_BSC_TESTNET;
            case 137:
                return HARDCODE_POLYGON_MAINNET;
            case 80002:
                return HARDCODE_POLYGON_TESTNET;
            default:
                return [];
        }
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
     * Loads the RPC list for a network.
     *  - Testnet: always use the hardcoded testnet list (no API, no localStorage).
     *  - Mainnet: fetch from API → cache to localStorage → fallback to hardcoded mainnet list.
     */
    private async _loadRpcList(network: 'bsc' | 'polygon'): Promise<string[]> {
        if (!this._isProduction) {
            const testnetFallback = network === 'bsc' ? HARDCODE_BSC_TESTNET : HARDCODE_POLYGON_TESTNET;
            this._logger.log(`${TAG} Testnet mode — using hardcoded ${network} testnet list (${testnetFallback.length} entries)`);
            return testnetFallback;
        }

        const storageKey = network === 'bsc' ? STORAGE_KEY_BSC : STORAGE_KEY_POLYGON;

        // 1. Try remote API (only when rpcHost is configured)
        if (this._rpcHost) {
            const endpoint = `${this._rpcHost}/${network}`;
            try {
                const controller = new AbortController();
                const abortTimer = setTimeout(() => controller.abort(), RPC_LIST_FETCH_TIMEOUT_MS);
                const response = await fetch(endpoint, {signal: controller.signal}).finally(() => clearTimeout(abortTimer));
                if (response.ok) {
                    const data = await response.json() as string[];
                    if (Array.isArray(data) && data.length > 0) {
                        localStorage.setItem(storageKey, JSON.stringify(data));
                        this._logger.log(`${TAG} Fetched ${network} RPC list from API (${data.length} entries)`);
                        return data;
                    }
                }
            } catch (e) {
                this._logger.error(`${TAG} Failed to fetch ${network} RPC list from API: ${e}`);
            }
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

        // 3. Hardcoded mainnet fallback — intentionally NOT saved to localStorage
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
                await this._probe(rpc);
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

    /**
     * Single eth_blockNumber call, by hand. Rejects when the node does not
     * answer with a usable result.
     *
     * A JsonRpcProvider was used here, and it brings two behaviors this step
     * does not want. It reads HTTP 429 as throttling and retries on its own,
     * backing off and obeying Retry-After, so a rate limited node kept the
     * probe busy after the timeout had already given up on it. Worse, when its
     * network detection fails it retries that every second - forever, and
     * destroy() does not stop it - so every probed bad node left a loop of
     * requests behind, which is what showed up in the network tab. The probe
     * only asks whether the node answers right now: one request, no retry,
     * nothing left running.
     */
    private async _probe(rpc: string): Promise<void> {
        const controller = new AbortController();
        const timer = setTimeout(() => controller.abort(), RPC_TEST_TIMEOUT_MS);
        try {
            const response = await fetch(rpc, {
                method: 'POST',
                headers: {'content-type': 'application/json'},
                body: JSON.stringify({jsonrpc: '2.0', id: 1, method: 'eth_blockNumber', params: []}),
                signal: controller.signal,
            });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const body = await response.json();
            if (body?.error || typeof body?.result !== 'string') {
                throw new Error(`bad response: ${JSON.stringify(body?.error ?? body)}`);
            }
        } finally {
            clearTimeout(timer);
        }
    }
}

