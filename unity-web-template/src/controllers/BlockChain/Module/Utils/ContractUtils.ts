import * as Storage from "./Storage.js";
import {Contract, ethers} from "ethers";

/**
 *
 * @param {string} address
 * @param {string} abi
 * @returns {Contract | null}
 */
function createReadContract(address: string, abi: JSON): Contract | null {
    if (address) {
        const provider = Storage.getBrowserProvider();
        return new ethers.Contract(address, JSON.stringify(abi), provider);
    }
    console.warn('Empty abi and address');
    return null;
}

/**
 *
 * @param {string} address
 * @param {string} abi
 * @returns {Contract | null}
 */
async function createWriteContract(address: string, abi: JSON): Promise<Contract | null> {
    if (address) {
        const userAddress = Storage.getUserAddress();
        const provider = Storage.getBrowserProvider();
        const signer = await provider.getSigner(userAddress);
        return new ethers.Contract(address, JSON.stringify(abi), signer);
    }
    console.warn('Empty abi and address');
    return null;
}

export {
    createReadContract,
    createWriteContract
}