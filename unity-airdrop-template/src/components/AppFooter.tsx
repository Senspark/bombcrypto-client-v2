import {useContext, useEffect} from "react";
import {StyleContext} from "./StyleContext.tsx";
import styled, {css} from 'styled-components';
import {useState} from "react";
import ConnectAccount from "./ConnectAccount.tsx";
import ConnectNetwork from "./ConnectNetwork.tsx";
import { ClipLoader } from "react-spinners";
import "./../App.css";
import { EnvConfig } from '../configs/EnvConfig';
import AppKitButton from "./AppKitButton";

const Wrapper = styled.div<{ disable: string; fullratio: string }>`
    height: 80px;
    pointer-events: ${({ disable, fullratio }) =>
    (disable === 'true' || fullratio === 'true') ? 'none' : 'auto'};
    display: ${({ disable, fullratio }) =>
    (disable === 'true' || fullratio === 'true') ? 'none' : 'block'};;
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

    opacity: ${({ state }) => {
    switch (state) {
        case 'fetching':
            return 0.7;
        default:
            return 'default';
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

const UserInfoWrapper = styled.div`
    border-radius: var(--wui-border-radius-3xl);
    background: var(--wui-color-gray-glass-002);
    display: flex;
    gap: var(--wui-spacing-xs);
    padding: var(--wui-spacing-3xs) var(--wui-spacing-xs);
    border: 1px solid var(--wui-color-gray-glass-005);
    justify-content: center;
    align-items: center;
`;

const UserInfoFrame = styled.div`
    display: flex;
    align-items: center;
    border-radius: var(--wui-border-radius-3xl);
    border: 1px solid var(--wui-color-gray-glass-005);
    background: var(--wui-color-gray-glass-005);
    padding: var(--wui-spacing-3xs) var(--wui-spacing-xs);
    text-rendering: optimizespeed;
    -webkit-font-smoothing: antialiased;
    -webkit-tap-highlight-color: transparent;
    gap: var(--wui-spacing-xxs, 8px); /* Adds space between avatar and text */
`;

const UserAvatar = styled.div`
    width: 20px;
    height: 20px;
    border-radius: 50%;

    background: radial-gradient(
        var(--local-radial-circle, 88% 88% at 65% 40%),
        #fff 0.52%,
        var(--mixed-local-color-5, rgb(161, 176, 181)) 31.25%,
        var(--mixed-local-color-3, rgb(91, 116, 125)) 51.56%,
        var(--mixed-local-color-2, rgb(55, 87, 97)) 65.63%,
        color-mix(in srgb, #fff 0%, rgb(20, 57, 69)) 82.29%,
        var(--mixed-local-color-4, rgb(126, 146, 153)) 100%
    );
`;

const UserName = styled.span`
    color: var(--wui-color-fg-200);
    font-family: var(--wui-font-family);    
    font-weight: var(--wui-font-weight-regular);
    font-size: var(--wui-font-size-paragraph);
`;

interface ButtonProps {
  state: 'fetching' | 'enable' | 'logging';
}

// Extend the window interface for TypeScript
declare global {
    interface Window {
        setConnectWallet: (val: boolean) => void;
        setConnectUser: (val: boolean) => void;
        setUserName: (val: string) => void;
        setLoginState: (val: 'fetching' | 'enable' | 'logging') => void;
        getLoginState: () => 'fetching' | 'enable' | 'logging';
        setOpenConnectNetwork: (val: boolean) => void;
    }
}

export default function AppFooter() {
    const styleContext = useContext(StyleContext);
    const [openConnectAccount, setOpenConnectAccount] = useState(false);
    const [openConnectNetwork, setOpenConnectNetwork] = useState(false);
    const [connectWallet, setConnectWallet] = useState(true);
    const [connectUser, setConnectUser] = useState(false);
    const [username, setUserName] = useState('');
    const [loginState, setLoginState] = useState<'fetching' | 'enable' | 'logging'>('fetching');
    
    //Bản này chỉ dùng cho RON và BASE ko có connect account nên tạm ignore
    const [ignoreAccountConnect] = useState(true);
    

    useEffect(() => {
        window.setConnectWallet = setConnectWallet;
        window.setConnectUser = setConnectUser;
        window.setUserName = setUserName;
        window.setOpenConnectNetwork = setOpenConnectNetwork;
    }, []);

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
        <Wrapper disable={(loginState === 'logging').toString()} fullratio={(fullratio).toString()}>
            <ButtonWrapper>
                {(connectWallet) && <AppKitButton />}
                <PlayOtherNetworkButton
                    onClick={() => window.open(EnvConfig.otherNetworkUrl(), '_blank', 'noopener,noreferrer')}
                >
                    Play on other network
                </PlayOtherNetworkButton>
                {(!connectUser) && (!ignoreAccountConnect) && (
                    <ConnectAccountButton state={loginState} onClick={loginState === 'enable' ? handleConnectAccountClicked : undefined}>
                        {loginState === 'fetching' &&
                            <>
                                <ClipLoader size="15px" color="#ffffff" />
                                <UserLoadingText color="#ffffff">Fetching data...</UserLoadingText>
                            </>
                        }
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
                {username && (
                    <UserInfoWrapper>
                        <UserInfoFrame>
                            <UserAvatar/>
                            <UserName>{username}</UserName>
                        </UserInfoFrame>
                    </UserInfoWrapper>
                )}
            </ButtonWrapper>
            <ConnectAccount open={openConnectAccount} setOpen={setOpenConnectAccount}/>
            <ConnectNetwork open={openConnectNetwork} setOpen={setOpenConnectNetwork}/>
        </Wrapper>
    );
}