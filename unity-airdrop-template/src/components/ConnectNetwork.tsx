import {useEffect, useRef} from "react";
import styled from 'styled-components';
import unityBridge from "../controllers/unity/UnityBridge.ts";

import bnbIcon from '@assets/images/webgame polygon/icon_bnb.png'
import polygonIcon from '@assets/images/webgame polygon/icon_polygon.png'

const FullScreenWrapper = styled.div<{ open: boolean }>`
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background-color: rgba(20, 20, 20, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;

    opacity: ${({ open }) => (open ? 1 : 0)};
    pointer-events: ${({ open }) => (open ? 'auto' : 'none')};
    transition: opacity 0.2s ease, transform 0.2s ease;
`;

const TopBar = styled.div`
    position: absolute;
    width: 100%;
    height: 70px;
    display: flex;
    align-items: center;
`;

const MiddleBar = styled.div`
    position: absolute;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    top: 80px;
    flex-direction: column;
    max-width: 380px;
    left: 50%;
    transform: translateX(-50%);
    gap: 15px;
`;

const TitleText = styled.div`
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    color: white;
    font-size: 20px;
    font-weight: bold;
`;

const NetworkBoardWrapper = styled.div`
    position: fixed;
    width: 420px; // Board width
    height: 260px; // Board height
    border-radius: 50px; // Rounded corners
    background-color: rgba(0, 0, 0, 1);
    margin-bottom: 300px;
`;

const StyledNetwork = styled.button`
    width: 100%;
    background-color: rgba(255, 255, 255, 0.1);
    height: 60px;
    border-radius: 6px; /* Equivalent to --wui-border-radius-xs */
    display: flex;
    align-items: center;
    cursor: pointer;
    outline: none;
    border: none;

    &:hover {
        background-color: rgba(255, 255, 255, 0.2);
    }
`;

const NetworkImage = styled.img<{ src: string }>`
    position: absolute;
    width: 45px;
    height: 45px;
    left: 80px;
`;

const NetworkText = styled.span`
    position: absolute;
    font-size: 18px;
    color: #fff;
    left: 140px;
`;

interface IProps {
    open: boolean;
    setOpen: (open: boolean) => void;
}

const ConnectNetwork = (props: IProps) => {
    const callbackMap = useRef<Record<string, any>>({});

    useEffect(() => {
        unityBridge.setUnityInput(!props.open);
        if (props.open) {
            handleOpen();
        } else {
            handleClose();
        }
    }, [props.open]);

    useEffect(() => {
        const handler = (event: CustomEvent) => {
            const type = event.type; // e.g., 'showLogin' or 'showNetwork'
            callbackMap.current[type] = event.detail;
        };

        window.addEventListener("showLogin", handler as EventListener);
        window.addEventListener("showNetwork", handler as EventListener);

        return () => {
            window.removeEventListener("showLogin", handler as EventListener);
            window.removeEventListener("showNetwork", handler as EventListener);
        };
    }, []);

    const handleContentClick = (e: React.MouseEvent) => {
        e.stopPropagation(); // ⛔️ Stops click from closing modal
    };

    const handleNetwork = (network: string) => {
        callbackMap.current["showNetwork"]?.onSubmit(network);

        window.enableBgVideo(false);
        props.setOpen(false);
    };

    const handleClose = () => {
        if (typeof window.setLoginState === 'function' &&
            typeof window.getLoginState === 'function' &&
            window.getLoginState() !== 'fetching'
        ) {
            window.setLoginState('enable');
        }
    };

    const handleOpen = () => {
        
    };

    const modal = (
        <FullScreenWrapper open={props.open}>
            <NetworkBoardWrapper onClick={handleContentClick}>
                <TopBar>
                    <TitleText>Choose Network</TitleText>
                </TopBar>
                <MiddleBar>
                    <StyledNetwork onClick={() => handleNetwork("Binance")}>
                        <NetworkImage src={bnbIcon}/>
                        <NetworkText>BNB Smart Chain</NetworkText>
                    </StyledNetwork>
                    <StyledNetwork onClick={() => handleNetwork("Polygon")}>
                        <NetworkImage src={polygonIcon}/>
                        <NetworkText>Polygon</NetworkText>
                    </StyledNetwork>
                </MiddleBar>
            </NetworkBoardWrapper>
        </FullScreenWrapper>
    );
    return (
        <>
            {modal}
        </>
    );
};

export default ConnectNetwork;