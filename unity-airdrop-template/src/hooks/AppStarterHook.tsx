import {useAppKitAccount, useAppKitNetwork, useAppKitProvider, useAppKit} from "@reown/appkit/react";
import {Provider} from "@reown/appkit-adapter-ethers";
import {useEffect} from "react";
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

    useEffect(() => {
        walletService.updateChainId(chainId, () => caipNetwork && switchNetwork(caipNetwork));
        close().then()
    }, [chainId, caipNetwork, switchNetwork, close]);

    return {
        isConnected,
        close,
    };
}