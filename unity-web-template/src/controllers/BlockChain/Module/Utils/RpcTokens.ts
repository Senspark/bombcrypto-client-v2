import RpcToken from "../RpcToken/RpcToken.ts";
import {getCustomRpc} from "../RpcToken/RpcAddress.ts";
import {RpcService} from "../../RpcService.ts";

type TokenData = {
    chainId: number;
    address: string;
    digit: number;
    index: number;
};

const SupportTokens: { [key: number]: RpcToken } = {};

// Tokens whose URLs come from the RpcService, so they can be refreshed once
// probing finishes. Ones pinned to a custom RPC are left out on purpose.
const ManagedTokens: Array<{ token: RpcToken, chainId: number }> = [];

// The service whose updates the tokens currently follow. Logging in again
// builds a new one, and the listener of the previous one must go quiet.
let listeningRpcService: RpcService | null = null;

/**
 *
 * @param tokensData - Array of token data objects
 * @param abi - The ABI of the token contract
 * @param rpcService
 */
function createRpcTokens(tokensData: TokenData[], abi: JSON, rpcService: RpcService): void {
    ManagedTokens.length = 0;

    for (let i = 0; i < tokensData.length; i++) {
        const d = tokensData[i];

        const address = d.address;
        const chainId = d.chainId;
        const digit = d.digit;
        const category = d.index;
        if (!address) {
            continue;
        }

        const customRpc = getCustomRpc(chainId);

        // A single URL used to be handed over here, picked while probing was
        // still running - so a rate limited node could be the only one this
        // token ever talked to. It now gets the whole list to rotate through,
        // and the verified subset as soon as the probe answers.
        const token = new RpcToken(address, abi, digit,
            customRpc.length > 0 ? customRpc : rpcService.getRpcs(chainId), chainId);
        SupportTokens[category] = token;

        if (customRpc.length === 0) {
            ManagedTokens.push({token, chainId});
        }
    }

    if (listeningRpcService !== rpcService) {
        listeningRpcService = rpcService;
        rpcService.onRpcsUpdated(() => {
            if (listeningRpcService !== rpcService) {
                return; // superseded by a newer login
            }
            for (const managed of ManagedTokens) {
                managed.token.setRpcUrls(rpcService.getRpcs(managed.chainId));
            }
        });
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


export {createRpcTokens, getBalance};