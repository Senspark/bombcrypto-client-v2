import GeneralContract from './GeneralContract.js';
import * as ContractUtils from "./ContractUtils.js";
import { Contract } from "ethers";

export default class NFTToken extends GeneralContract {
    private readonly _designAbi: JSON;
    private _designAddress?: string;
    private _designContract: Contract | null = null;

    /**
     *
     * @param address - The address of the contract
     * @param abi - The ABI of the contract
     * @param designAbi - The ABI of the design contract
     */
    constructor(address: string, abi: JSON, designAbi: JSON) {
        super(address, abi);
        this._designAbi = designAbi;
    }

    /**
     *
     * @returns A promise that resolves to the design contract
     */
    async getDesignContract(): Promise<Contract> {
        const contract = await this.getContract();
        if(!contract){
            throw new Error("Can not create contract");
        }
        if (!this._designAddress) {
            const designAddress = await contract.design();
            this._designAddress = designAddress;
            this._designContract = ContractUtils.createReadContract(designAddress, this._designAbi);
        }
        return this._designContract!;
    }
}