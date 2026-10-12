import {useAppKitAccount, useAppKitNetwork, useAppKitProvider, useAppKit} from "@reown/appkit/react";
import {Provider} from "@reown/appkit-adapter-ethers";
import {useEffect, useRef} from "react";
import {walletService} from "./GlobalServices.ts";

export const useAppService = () => {
    const {address, isConnected} = useAppKitAccount();
    const {chainId,caipNetwork, switchNetwork} = useAppKitNetwork();
    const {walletProvider} = useAppKitProvider<Provider>('eip155');
    const { close } = useAppKit();

    useEffect(() => {
        walletService.updateWalletAddress(address);
    }, [address]);

    useEffect(() => {
        walletService.updateWalletProvider(walletProvider);
    }, [walletProvider]);

    useEffect(() => {
        walletService.updateWalletConnection(isConnected);
    }, [isConnected]);

    // switchNetwork/close are new functions on every render, keep them out of the deps
    const latest = useRef({caipNetwork, switchNetwork, close});
    latest.current = {caipNetwork, switchNetwork, close};

    // Only when AppKit's network really changes: on other renders its chainId can lag
    // behind the wallet, and ensureCorrectChain would pull the wallet back
    useEffect(() => {
        walletService.updateChainId(chainId, () => {
            const {caipNetwork, switchNetwork} = latest.current;
            if (caipNetwork) {
                switchNetwork(caipNetwork);
            }
        });
        // AppKit's auto-switch after connect doesn't reliably add missing networks
        // (no popup for wallet_addEthereumChain). Reconcile with our own logic.
        walletService.ensureCorrectChain();
        latest.current.close().then()
    }, [chainId]);

    return {
        isConnected,
        close,
    };
}