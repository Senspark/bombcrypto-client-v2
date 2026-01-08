import {useCallback, useEffect, useRef, useState} from 'react';
import {useTonConnectUI} from "@tonconnect/ui-react";

import {AlertApi} from "../apis/AlertApi.ts";
import {authApi, logger, tonService} from "../services/GlobalServices.ts";

const TAG = '[ConnectWallet]';

type Props = {
    setConnected: (connected: boolean) => void;
}

export const ConnectWallet = (props: Props) => {
    const firstProofLoading = useRef<boolean>(true);

    const [authorized, setAuthorized] = useState(false);
    const [, setWalletAddress] = useState<string | null>(null);
    const [tonConnectUI] = useTonConnectUI();
    const tonConnectUISubcribed = useRef<boolean>(false);

    const recreateProofPayload = useCallback(async () => {
        const account = authApi.getAccountTon();
        if (!account || !account.jwt || !account.address || !account.publicKey) {
            loginAgain().then();
            return;
        }
        if (firstProofLoading.current) {
            authApi.scheduleRefreshJwt();
            firstProofLoading.current = false;
        }
    }
    , [tonConnectUI, firstProofLoading]);

    const loginAgain = async () => {
        if (firstProofLoading.current) {
            tonConnectUI.setConnectRequestParameters({state: 'loading'});
            firstProofLoading.current = false;
        }
        authApi.clear();
        setAuthorized(false);
        const payload = await authApi.getNonce();

        if (payload) {
            tonConnectUI.setConnectRequestParameters({state: 'ready', value: payload});
            if (tonConnectUI.modal.state.status != 'opened') {
                if (tonConnectUI.connected) {
                    await tonConnectUI.disconnect();
                }
                await tonConnectUI.openModal();
            }
        } else {
            tonConnectUI.setConnectRequestParameters(null);
        }
        tonService.setTonConnect(tonConnectUI);
    }

    const retryConnect = () => {
        recreateProofPayload().then()
    };

    if (firstProofLoading.current) {
        retryConnect();
    }

    useEffect(() => {
        if (tonConnectUISubcribed.current) {
            return;
        }
        tonConnectUISubcribed.current = true;
        const unsubscribe = tonConnectUI.onStatusChange(async connectedWallet => {
            try {
                logger.log(`${TAG} on status wallet change ${tonConnectUI.modalState.status}`);

                if (!connectedWallet) {
                    setAuthorized(false);
                    logger.error('Could not connect to your wallet')
                    return;
                }

                //Có jwt r nên skip các bước này
                if (authApi.getAccountTon().jwt)
                    return;

                if (connectedWallet.connectItems?.tonProof && 'proof' in connectedWallet.connectItems.tonProof) {
                    logger.log(`${TAG} checkProof`);
                    authApi.unScheduleRefreshJwt();
                    await authApi.checkProof(connectedWallet.connectItems.tonProof.proof, connectedWallet.account);


                    if (!authApi.getAccountTon().jwt) {
                        setAuthorized(false);
                        AlertApi.show('Authorization failed');
                        await Disconnect();
                        return;
                    }

                    props.setConnected(true);
                    setWalletAddress(authApi.getAccountTon().address);
                    setAuthorized(true);
                }
                authApi.scheduleRefreshJwt();

            } catch (e) {
                logger.error(`Wallet status change error: ${e}`);
                if (e && e instanceof Error) {
                    AlertApi.show(e.message);
                }
                authApi.clear();
                retryConnect();
            }
        });

        return () => {
            // Important: unsubscribe when component unmounts
            unsubscribe();
            tonConnectUISubcribed.current = false;
        };
    }, [tonConnectUI, props]);

    if (!authorized) {
        return null;
    }

    return (
        <></>
    );

    async function Disconnect() {
        if (tonConnectUI.connected) {
            await tonConnectUI.disconnect();
        }
    }
};
