import { Contract, formatUnits, JsonRpcProvider,} from 'ethers'
import {sleep} from "../../../../utils/Time.ts";

export default class RpcToken {
    private readonly _address: string;
    private readonly _abi: JSON;
    private readonly _digit: number;
    private readonly _rpcUrls: string[];
    private _contract: Contract | null = null;

    /**
     * @param {string} address
     * @param {string} abi
     * @param {number} digit
     * @param {string[]} rpcUrls
     */
    constructor(address: string, abi: JSON, digit: number, rpcUrls: string[]) {
        this._address = address;
        this._abi = abi;
        this._digit = digit;
        this._rpcUrls = rpcUrls;
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
                this._contract = null;
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
        
        const provider = new JsonRpcProvider(rpc);
        if(!provider){
            throw new Error("Can not create provider");
        }

        this._contract = new Contract(this._address, JSON.stringify(this._abi), provider);
    }
}