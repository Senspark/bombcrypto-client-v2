import { Contract, formatUnits, JsonRpcProvider, Network,} from 'ethers'
import {sleep} from "../../../../utils/Time.ts";

export default class RpcToken {
    private readonly _address: string;
    private readonly _abi: JSON;
    private readonly _digit: number;
    private readonly _rpcUrls: string[];
    private readonly _chainId: number;
    private _contract: Contract | null = null;
    private _provider: JsonRpcProvider | null = null;

    /**
     * @param {string} address
     * @param {string} abi
     * @param {number} digit
     * @param {string[]} rpcUrls
     * @param {number} chainId
     */
    constructor(address: string, abi: JSON, digit: number, rpcUrls: string[], chainId: number) {
        this._address = address;
        this._abi = abi;
        this._digit = digit;
        this._rpcUrls = rpcUrls;
        this._chainId = chainId;
    }

    /**
     * Replaces the URLs this token rotates through. Tokens are created before
     * the RPC probing finishes, so they start with unverified candidates and
     * get the verified ones here as soon as they are known.
     * @param {string[]} rpcUrls
     */
    setRpcUrls(rpcUrls: string[]): void {
        if (rpcUrls.length === 0) {
            return;
        }
        this._rpcUrls.splice(0, this._rpcUrls.length, ...rpcUrls);
        // Dropped so the next call rebuilds against the new list instead of
        // sticking to a provider that may be the node that just failed.
        this._dropContract();
    }

    /**
     * Releases the current provider before letting go of it.
     *
     * Simply setting _contract to null used to leave the provider alive: when
     * its network detection fails, ethers retries it every second on its own,
     * forever. Each rotation added one more of those, so a bad node ended up
     * being polled in a loop by providers nobody used anymore.
     */
    private _dropContract(): void {
        this._contract = null;
        if (this._provider) {
            this._provider.destroy();
            this._provider = null;
        }
    }

    /**
     * @param {string} userAddress
     * @returns {Promise<string>}
     */
    async getBalance(userAddress: string): Promise<string> {
        let tried = 10;
        while (tried > 0) {
            try {
                const c = await this._getContract();
                const v = await c.balanceOf(userAddress);
                return formatUnits(v.toString(), this._digit);
            } catch (error) {
                console.log(error);
                this._dropContract();
            }
            tried--;
            await sleep(300);
        }
        return "0";
    }

    private async _getContract(): Promise<Contract> {
        if (!this._contract) {
            await this._recreateContract();
        }
        if(!this._contract){
            throw new Error("Can not create contract");
        }
        return this._contract;
    }

    private async _recreateContract(): Promise<void> {
        const rpc = this._rpcUrls[0];
        this._rpcUrls.splice(0, 1);
        this._rpcUrls.push(rpc);
        console.log("create rpc: " + rpc);
        
        // The network is given instead of detected: on a node that is failing,
        // detection is retried by ethers every second forever - destroy() does
        // not stop it - so each rotation left another loop of requests running
        // against a node nobody was using anymore.
        const provider = new JsonRpcProvider(rpc, Network.from(this._chainId), {staticNetwork: true});
        if(!provider){
            throw new Error("Can not create provider");
        }

        this._provider = provider;
        this._contract = new Contract(this._address, JSON.stringify(this._abi), provider);
    }
}