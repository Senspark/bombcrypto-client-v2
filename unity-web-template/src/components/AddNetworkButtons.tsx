import { useState } from "react";
import styled from "styled-components";
import { EnvConfig } from "../configs/EnvConfig";
import {
    BNB_TESTNET_RPC,
    POLYGON_TESTNET_RPC,
    RONIN_TESTNET_RPC,
    BASE_TESTNET_RPC,
    NetworkRpc,
} from "../controllers/RpcNetworkUtils";

const Button = styled.button<{ disabled?: boolean }>`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border: 1px solid var(--wui-color-gray-glass-010);
    border-radius: var(--wui-border-radius-m);
    padding: 10px 16px;
    min-height: 40px;
    font-size: 16px;
    font-weight: 500;
    color: #fff;
    cursor: ${(p) => (p.disabled ? "default" : "pointer")};
    background-color: ${(p) => (p.disabled ? "#666" : "#f0b90b")};
    transition: background 0.2s;
    &:hover {
        background-color: ${(p) => (p.disabled ? "#666" : "#d6a309")};
    }
`;

type EthRequest = (args: { method: string; params?: unknown[] }) => Promise<unknown>;

function getInjectedProvider(): { request: EthRequest } | undefined {
    const w = window as unknown as { ethereum?: { request?: EthRequest } };
    if (w.ethereum?.request) return w.ethereum as { request: EthRequest };
    return undefined;
}

async function addOne(eth: { request: EthRequest }, rpc: NetworkRpc): Promise<boolean> {
    try {
        await eth.request({
            method: "wallet_addEthereumChain",
            params: [
                {
                    chainId: rpc.chainIdHex,
                    rpcUrls: [rpc.rpcUrl],
                    chainName: rpc.chainName,
                    nativeCurrency: {
                        name: rpc.currencySymbol,
                        symbol: rpc.currencySymbol,
                        decimals: rpc.decimals,
                    },
                    blockExplorerUrls: [rpc.blockExplorerUrl],
                },
            ],
        });
        return true;
    } catch (e) {
        console.warn("[AddNetworkButtons] add failed", rpc.chainName, e);
        return false;
    }
}

const TESTNET_RPCS: NetworkRpc[] = [
    BNB_TESTNET_RPC,
    POLYGON_TESTNET_RPC,
    RONIN_TESTNET_RPC,
    BASE_TESTNET_RPC,
];

const AddNetworkButtons: React.FC = () => {
    const [busy, setBusy] = useState(false);

    if (EnvConfig.isProduction()) return null;

    const handleClick = async () => {
        if (busy) return;
        const eth = getInjectedProvider();
        if (!eth) {
            alert("No injected wallet detected. Install MetaMask (or similar) and reload.");
            return;
        }
        setBusy(true);
        // Sequential: MetaMask shows one popup at a time. User clicks Approve N times.
        for (const rpc of TESTNET_RPCS) {
            await addOne(eth, rpc);
        }
        setBusy(false);
    };

    return (
        <Button disabled={busy} onClick={handleClick}>
            {busy ? "Adding..." : "Add Test Networks"}
        </Button>
    );
};

export default AddNetworkButtons;
