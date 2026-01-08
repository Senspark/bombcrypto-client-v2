import GeneralContract from './Utils/GeneralContract.js';
import { getDoubleGasFeeOption } from "./Utils/NetworkUtils.js";


export default class AirDrop extends GeneralContract {
    async getNFT(amount: number, eventId: string, nonce: string, signature: string): Promise<boolean> {
        try {
            const contract = await this.getContract();
            if(!contract){
                console.error('contract is null');
                return false;
            }
            const estimateGas: bigint = await contract.claimNFT.estimateGas(amount, eventId, nonce, signature);
            const options = getDoubleGasFeeOption(estimateGas);
            await contract.claimNFT(amount, eventId, nonce, signature, options);
            return true;
        } catch (ex) {
            console.error(`exception ${ex}`);
            return false;
        }
    }
}