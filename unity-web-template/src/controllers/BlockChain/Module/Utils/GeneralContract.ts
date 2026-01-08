import { Contract } from 'ethers';
import * as ContractUtils from './ContractUtils.js';

export default class GeneralContract {
    readonly _address: string;
    protected readonly _abi: JSON;
    private _contract: Contract | null = null;

    /**
     * @param {string} address
     * @param {string} abi
     */
    constructor(address: string, abi: JSON) {
        this._address = address;
        this._abi = abi;
    }

    /**
     * @returns {Promise<Contract | null>}
     */
    async getContract(): Promise<Contract> {
        if (!this._contract) {
            this._contract = await ContractUtils.createWriteContract(this._address, this._abi);
        }
        if (!this._contract) {
            throw new Error('Can not create contract');
        }
        return this._contract;
    }
}