import {JsonRpcProvider, Network} from "ethers";
import {RpcService} from "../../RpcService.ts";
import {getCustomRpc} from "../RpcToken/RpcAddress.ts";
import {getRpcByChainId} from "../../../RpcNetworkUtils.ts";

// Chains whose RPC list is managed by RpcService; the rest use the built-in URL
const MANAGED_CHAINS = [56, 97, 137, 80002];

const providers = new Map<number, JsonRpcProvider>();
let rpcService: RpcService | null = null;

function setRpcService(service: RpcService): void {
    rpcService = service;
    providers.clear();
    service.onRpcsUpdated(() => {
        if (rpcService === service) {
            providers.clear();
        }
    });
}

function pickUrl(chainId: number): string {
    const custom = getCustomRpc(chainId);
    if (custom.length > 0) {
        return custom[0];
    }
    if (rpcService && MANAGED_CHAINS.includes(chainId)) {
        const url = rpcService.getRpc(chainId);
        if (url) {
            return url;
        }
    }
    const rpc = getRpcByChainId(chainId);
    if (!rpc) {
        throw new Error(`No RPC for chain ${chainId}`);
    }
    return rpc.rpcUrl;
}

// Reads never go through the wallet: its network is shared by every tab and
// may belong to another tab's game right now.
function getReadProvider(chainId: number): JsonRpcProvider {
    let provider = providers.get(chainId);
    if (!provider) {
        provider = new JsonRpcProvider(pickUrl(chainId), Network.from(chainId), {staticNetwork: true});
        providers.set(chainId, provider);
    }
    return provider;
}

// Forget a failing node so the next read picks another one
function dropReadProvider(chainId: number): void {
    providers.delete(chainId);
}

export {setRpcService, getReadProvider, dropReadProvider};
