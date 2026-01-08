
import { shuffle } from "./Utils.js";
import RpcToken from "../RpcToken/RpcToken.ts";
import getAllRpc, {getCustomRpc} from "../RpcToken/RpcAddress.ts";

type TokenData = {
    chainId: number;
    address: string;
    digit: number;
    index: number;
};

const SupportTokens: { [key: number]: RpcToken } = {};

/**
 *
 * @param tokensData - Array of token data objects
 * @param abi - The ABI of the token contract
 */
function createRpcTokens(tokensData: TokenData[], abi: JSON): void {
    for (let i = 0; i < tokensData.length; i++) {
        const d = tokensData[i];

        const address = d.address;
        const chainId = d.chainId;
        const digit = d.digit;
        const category = d.index;
        if (!address) {
            continue;
        }

        let rpcUrls = getCustomRpc(chainId);
        if (rpcUrls.length === 0) {
            rpcUrls = shuffle(getAllRpc(chainId));
        }
        SupportTokens[category] = new RpcToken(address, abi, digit, rpcUrls);
    }
}

/**
 *
 * @param category - The category of the token
 * @param userAddress - The address of the user
 * @returns A promise that resolves to the balance of the user
 */
async function getBalance(category: number, userAddress: string): Promise<string> {
    const t = SupportTokens[category];
    const v = await t.getBalance(userAddress);
    return v;
}


export { createRpcTokens, getBalance };