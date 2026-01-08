import {useAppKitAccount, useAppKitProvider} from "@reown/appkit/react";
import {Provider} from "@reown/appkit-adapter-solana";
import {useEffect} from "react";
import {walletService} from "./GlobalServices.ts";
import {useAppKitConnection} from "@reown/appkit-adapter-solana/react";

export const useAppService = () => {

    const {address, isConnected} = useAppKitAccount()
    const {walletProvider} = useAppKitProvider<Provider>('solana');
    const {connection} = useAppKitConnection();

    useEffect(() => {
        walletService.updateWalletAddress(address);
    }, [address]);

    useEffect(() => {
        walletService.updateWalletProvider(walletProvider);
    }, [walletProvider]);

    useEffect(() => {
        walletService.updateWalletConnection(connection);
    }, [connection]);

    return {
        isConnected,
    };
}