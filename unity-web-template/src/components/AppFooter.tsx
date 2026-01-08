import {useContext, useEffect, useCallback} from "react";
import {StyleContext} from "./StyleContext.tsx";
import styled, {css, keyframes} from 'styled-components';
import {useState} from "react";
import ConnectAccount from "./ConnectAccount.tsx";
import ConnectNetwork from "./ConnectNetwork.tsx";
import { ClipLoader } from "react-spinners";
import "./../App.css";
import { EnvConfig } from '../configs/EnvConfig';
import AppKitButton from "./AppKitButton";
import {customSessionStorage, sessionSetting} from "../hooks/GlobalServices.ts";

const Wrapper = styled.div<{ fullratio: string }>`
    height: 80px;
    pointer-events: ${({ fullratio }) =>
    (fullratio === 'true') ? 'none' : 'auto'};
    display: ${({ fullratio }) =>
    (fullratio === 'true') ? 'none' : 'block'};
    position: relative;
    background: rgba(0, 0, 0, 255);
`;

const ButtonWrapper = styled.div`
    height: 80px;
    pointer-events: 'auto';
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
`;

// Define animations for the loading UI
const pulse = keyframes`
  0% {
    transform: scale(0.95);
    opacity: 0.7;
  }
  50% {
    transform: scale(1);
    opacity: 1;
  }
  100% {
    transform: scale(0.95);
    opacity: 0.7;
  }
`;

const rotate = keyframes`
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
`;

const fadeIn = keyframes`
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
`;

const LoadingWrapper = styled.div`
    height: 80px;
    display: flex;
    justify-content: center;
    align-items: center;
    background: rgba(0, 0, 0, 255);
    animation: ${fadeIn} 0.3s ease-in;
`;

const LoadingContent = styled.div`
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: row;
    gap: 15px;
    animation: ${pulse} 2s infinite ease-in-out;
`;

const LoadingText = styled.div`
    color: #f77440;
    font-family: var(--wui-font-family);
    font-weight: 500;
    font-size: 16px;
`;

const LoadingSpinner = styled.div`
    position: relative;
    width: 30px;
    height: 30px;
    display: flex;
    justify-content: center;
    align-items: center;
    
    &:before {
        content: '';
        position: absolute;
        width: 100%;
        height: 100%;
        border-radius: 50%;
        border: 3px solid transparent;
        border-top-color: #f77440;
        animation: ${rotate} 1.5s linear infinite;
    }
    
    &:after {
        content: '';
        position: absolute;
        width: 60%;
        height: 60%;
        border-radius: 50%;
        border: 3px solid transparent;
        border-top-color: #f77440;
        border-bottom-color: #f77440;
        opacity: 0.7;
        animation: ${rotate} 2s linear infinite reverse;
    }
`;

const SpinnerCore = styled.div`
    width: 20%;
    height: 20%;
    background-color: #f77440;
    border-radius: 50%;
    z-index: 2;
`;

const ConnectAccountButton = styled.button<ButtonProps>`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px; /* space between spinner and text */

    border: 1px solid var(--wui-color-gray-glass-010);
    border-radius: var(--wui-border-radius-m);
    padding: 10px 16px;
    font-size: 16px;
    font-weight: 500;
    color: var(--wui-color-foreground-100, #fff);
    cursor: pointer;

    background-color: ${({ state }) => {
        switch (state) {
        case 'logging':
            return 'var(--wui-color-gray-glass-015)';
        default:
            return '#f77440';
        }
    }};

    ${({ state }) =>
    state === 'enable' &&
    css`
      &:hover {
        background-color: #e7632f;
      }
    `}
`;

const PlayOtherNetworkButton = styled.button`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border: 1px solid var(--wui-color-gray-glass-010);
    border-radius: var(--wui-border-radius-m);
    padding: 10px 16px;
    font-size: 16px;
    font-weight: 500;
    color: var(--wui-color-foreground-100, #fff);
    cursor: pointer;
    background-color: #22c55e; /* green-500 */
    transition: background 0.2s;
    &:hover {
        background-color: #15803d; /* green-700 */
    }
`;

const UserLoadingText = styled.span<{ color: string}>`
    color: ${({ color }) => color};
    font-family: var(--wui-font-family);    
    font-weight: var(--wui-font-weight-regular);
    font-size: var(--wui-font-size-paragraph);
`;

interface ButtonProps {
  state: 'enable' | 'logging';
}

// Extend the window interface for TypeScript
declare global {
    interface Window {
        setUseAccount: (val: boolean) => void;
        setLoginState: (val: 'enable' | 'logging') => void;
        getLoginState: () => 'enable' | 'logging';
        setOpenConnectNetwork: (val: boolean) => void;
        showFooterContent: (show: boolean) => void;
    }
}

export default function AppFooter() {
    const styleContext = useContext(StyleContext);
    const [openConnectAccount, setOpenConnectAccount] = useState(false);
    const [openConnectNetwork, setOpenConnectNetwork] = useState(false);
    const [useAccount, setUseAccount] = useState(false);
    const [loginState, setLoginState] = useState<'enable' | 'logging'>('enable');
    const [showContent, setShowContent] = useState(false);

    // Create a callback function to check wallet session
    const checkWalletSession = useCallback(async () => {
        const isShow = await customSessionStorage.get(sessionSetting.getSessionKey().isUseWallet);
        if(isShow !== 'true') {
            window.setUseAccount(true);
            }
        }, []);

    useEffect(() => {
        window.setUseAccount = (value: boolean) => {
            setUseAccount(value);
            setLoginState('enable');
        };
        window.setOpenConnectNetwork = setOpenConnectNetwork;
        window.showFooterContent = (show: boolean) => {
            setShowContent(show);
        };

        //Ko set cái này dưa trên connect wallet nữa mà dựa trên session có dùng wallet hay ko
        // Mới load thì check session luôn để biết có show button connect account hay ko
        checkWalletSession().then();
    }, [checkWalletSession]);

    useEffect(() => {
        window.setLoginState = setLoginState;
        window.getLoginState = () => loginState;
    }, [loginState]);

    const handleConnectAccountClicked = () => {
        setLoginState('logging');
        setOpenConnectAccount(true);
    };

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const { fullratio } = styleContext;
    if (fullratio) {
        // return null;
    }

    return (
        <Wrapper fullratio={(fullratio).toString()}>
            {!showContent ? (
                <LoadingWrapper>
                    <LoadingContent>
                        <LoadingSpinner>
                            <SpinnerCore />
                        </LoadingSpinner>
                        <LoadingText>Loading...</LoadingText>
                    </LoadingContent>
                </LoadingWrapper>
            ) : (
                <ButtonWrapper>
                    {<AppKitButton />}
                    {( (useAccount)) && (
                        <ConnectAccountButton state={loginState} onClick={loginState === 'enable' ? handleConnectAccountClicked : undefined}>
                            {loginState === 'enable' &&
                                <UserLoadingText color="#ffffff">Login with Senspark</UserLoadingText>
                            }
                            {loginState === 'logging' && (
                                <>
                                    <ClipLoader size="15px" color="#f77440" />
                                    <UserLoadingText color="#f77440">Logging in...</UserLoadingText>
                                </>
                            )}
                        </ConnectAccountButton>
                    )}
                    { (
                        <PlayOtherNetworkButton onClick={() => window.open(EnvConfig.otherNetworkUrl(), '_blank', 'noopener,noreferrer')} >
                            Play on other network
                        </PlayOtherNetworkButton>
                    )}
                </ButtonWrapper>
            )}
            <ConnectAccount open={openConnectAccount} setOpen={setOpenConnectAccount}/>
            <ConnectNetwork open={openConnectNetwork} setOpen={setOpenConnectNetwork}/>
        </Wrapper>
    );
}