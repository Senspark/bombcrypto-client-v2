import {useEffect, useState, useRef} from "react";
import {EnvConfig} from "../configs/EnvConfig.ts";
import styled from 'styled-components';
import {useLocalStorageState} from "ahooks";
import { MdClose } from 'react-icons/md';
import { authService } from "../hooks/GlobalServices.ts";

const CREATE_ACCOUNT_URL_PROD = "https://dapp.bombcrypto.io/account/register";
const CREATE_ACCOUNT_URL_TEST = "";
const FORGOT_PASSWORD_URL_PROD = "https://dapp.bombcrypto.io/account/login";
const FORGOT_PASSWORD_URL_TEST = "";

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

const LoginBoardWrapper = styled.div`
    position: fixed;
    width: 420px; // Board width
    height: 450px; // Board height
    border-radius: 50px; // Rounded corners
    background-color: #121313;
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

const BottomBar = styled.div`
    position: absolute;
    width: 100%;
    height: 150px;
    bottom: 0;
    display: flex;
    align-items: center;
`;

const TitleText = styled.div`
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    color: white;
    font-size: 20px;
    font-weight: bold;
`;

const PositionButton = styled.button<PositionButtonProps>`
    position: absolute;
    ${(props) => (props.position === "right" ? "right: 20px;" : "left: 20px;")}
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--wui-color-fg-100);
    cursor: pointer;
    outline: none;
    border: none;
    background-color: transparent;
`;

const StyledInput = styled.input`
    width: 100%;
    padding: 15px 25px;
    border-radius: 6px; /* Equivalent to --wui-border-radius-xs */
    box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.1); /* matches gray glass look */
    background-color: rgba(255, 255, 255, 0.05); /* mimic --wui-color-gray-glass-002 */
    font-size: 16px;
    letter-spacing: 0.5px; /* mimic --wui-letter-spacing-paragraph */
    color: white; /* --wui-color-fg-100 */
    border: none;
    outline: none;

    &::placeholder {
        color: rgba(255, 255, 255, 0.4);
    }

    &:focus {
        box-shadow: inset 0 0 0 2px #007bff;
        background-color: rgba(255, 255, 255, 0.08);
    }
`;

const StyledError = styled.div`
    width: 100%;
    background-color: #fdd4e1;
    height: 35px;
    border-radius: 6px; /* Equivalent to --wui-border-radius-xs */
    display: flex;
    align-items: center;
    justify-content: center;
`;

const ErrorText = styled.div`
    color: #a90206;
    font-size: 14px;
`;

const StyleOption = styled.div`
    width: 100%;
    font-size: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
`;

const RememberMe = styled.label`
    color: rgba(255, 255, 255, 0.4);
    display: flex;
    align-items: center;
    gap: 8px;
`;

const ForgotPassword = styled.span`
  color: rgba(255, 255, 255, 0.4);
  cursor: pointer;
`;

const LoginButton = styled.button`
    position: absolute;
    width: 380px;
    height: 40px;
    top: 0px;
    left: 50%;
    transform: translateX(-50%);
    background-color: #f77440;
    color: white;
    border: none;
    border-radius: 15px;
    font-size: 16px;
    font-weight: bold;
    text-align: center;
    cursor: pointer;

    &:hover {
        background-color: #e7632f;
    }
`;

const DividerContainer = styled.div`
    position: absolute;
    bottom: 50px;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    align-items: center;
    width: 100%;
    color: white;
`;

const Line = styled.div`
    flex: 1;
    height: 1px;
    background-color: white;
    opacity: 0.3;
`;

const Text = styled.span`
    padding: 0 12px;
    font-size: 14px;
    color: white;
    opacity: 0.7;
`;

const FooterWrapper = styled.div`
    display: inline-flex;
    position: absolute;
    bottom: 20px;
    left: 50%;
    transform: translateX(-50%);
    align-items: center;
    gap: 10px;
    font-family: var(--wui-font-family);
`;

const InfoText = styled.span`
    color: var(--wui-color-fg-200);
    white-space: nowrap;
`;

const CreateAccount = styled.a`
    color: #5566c9;
    text-decoration: none;
    cursor: pointer;
    white-space: nowrap;
`;

interface PositionButtonProps {
    position?: "left" | "right";
}

interface IProps {
    open: boolean;
    setOpen: (open: boolean) => void;
}

const ConnectAccount = (props: IProps) => {
    const callbackMap = useRef<Record<string, any>>({});

    useEffect(() => {
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

    const handleCreateAccount = () => {
        const url = EnvConfig.isProduction()
        ? CREATE_ACCOUNT_URL_PROD
        : CREATE_ACCOUNT_URL_TEST;
        window.open(url, "_blank", "noopener,noreferrer");
    };

    const handleForgotPassword = () => {
        const url = EnvConfig.isProduction()
        ? FORGOT_PASSWORD_URL_PROD
        : FORGOT_PASSWORD_URL_TEST;
        window.open(url, "_blank", "noopener,noreferrer");
    };

    const handleLogin = () => {
        if (!usernameInput.trim() || !passwordInput.trim()) {
            setErrorMessage("Please enter the required input fields");
            return;
        }

        const usernameRegex = /^[a-zA-Z0-9]{6,20}$/;
        const passwordRegex = /^[^\s]{6,20}$/;
        if (!usernameRegex.test(usernameInput) || !passwordRegex.test(passwordInput)) {
            setErrorMessage("Incorrect username or password. Please try again");
            return;
        }
        
        authService.setAccountData({ 
                userName: usernameInput, 
                password: passwordInput,
                network: null,
            });
        props.setOpen(false);
    };

    const handleClose = () => {
        const shouldRemember = rememberMe;
            setRememberMe(shouldRemember);
            if (localStorage.getItem('rememberMe') === 'true') {
                // Save input fields to localStorage
                localStorage.setItem('accUsername', usernameInput);
                localStorage.setItem('accPassword', passwordInput);
                setUsername(usernameInput);
                setPassword(passwordInput);
            } else {
                // Clear localStorage
                localStorage.removeItem('accUsername');
                localStorage.removeItem('accPassword');
                setUsername(undefined);
                setPassword(undefined);
                // Clear inputs
                setUsernameInput('');
                setPasswordInput('');
            }
            
            setErrorMessage('');
    };

    const resetLoginState = () => {
        props.setOpen(false);
        if (typeof window.setLoginState === 'function') {
            window.setLoginState('enable');
        }
    }

    const handleOpen = () => {
        // Restore data from storage only if rememberMe is true
        const shouldRemember = rememberMe;
        setRememberMe(shouldRemember);
        if (localStorage.getItem('rememberMe') === 'true') {
            setUsernameInput(localStorage.getItem('accUsername') ?? '');
            setPasswordInput(localStorage.getItem('accPassword') ?? '');
        } else {
            setUsername(undefined);
            setPassword(undefined);
            setUsernameInput('');
            setPasswordInput('');
        }
    };

    const storageOptions = {
        serializer: (value: string | undefined) => value ?? ''
        // serializer: (value: string | undefined) => {
        //     return value?.replace(/^"|"$/g, '') ?? ''; // remove quotes
        // },
    };

    const [errorMessage, setErrorMessage] = useState('');
    const [rememberMe, setRememberMe] = useLocalStorageState<string | undefined>('rememberMe', storageOptions);

    const [username, setUsername] = useLocalStorageState<string | undefined>('accUsername', storageOptions);
    const [password, setPassword] = useLocalStorageState<string | undefined>('accPassword', storageOptions);
    const [usernameInput, setUsernameInput] = useState(username || '');
    const [passwordInput, setPasswordInput] = useState(password || '');

    const modal = (
        <FullScreenWrapper open={props.open} onClick={() => {
            resetLoginState();
            }
        }>
            <LoginBoardWrapper onClick={handleContentClick}>
                <TopBar>
                    <TitleText>Login with Senspark</TitleText>
                    <PositionButton position="right" onClick={() => {
                        resetLoginState();
                        }
                    }>
                        <MdClose name="close" size="md" color="inherit" />
                    </PositionButton>
                </TopBar>
                <MiddleBar>
                    <StyledInput
                        type="text"
                        placeholder="Username"
                        value={usernameInput}
                        onChange={(e) => {
                            setUsernameInput(e.target.value);
                            setErrorMessage('');
                        }}
                    />
                    <StyledInput
                        type="password"
                        placeholder="Password"
                        value={passwordInput}
                        onChange={(e) => {
                            setPasswordInput(e.target.value);
                            setErrorMessage('');
                        }}
                    />
                    {errorMessage && 
                    <StyledError>
                        <ErrorText>{errorMessage}</ErrorText>
                    </StyledError>
                    }
                    <StyleOption>
                        <RememberMe>
                            <input 
                                type="checkbox"
                                checked={localStorage.getItem('rememberMe') === 'true'}
                                onChange={(e) => setRememberMe(e.target.checked ? 'true' : 'false')}
                            />
                            <span>Remember me</span>
                        </RememberMe>
                        <ForgotPassword onClick={handleForgotPassword}>
                            Forgot password?
                        </ForgotPassword>
                    </StyleOption>
                </MiddleBar>
                <BottomBar>
                    <LoginButton onClick={handleLogin}>
                        Login
                    </LoginButton>
                    <DividerContainer>
                        <Line />
                            <Text>or</Text>
                        <Line />
                    </DividerContainer>
                    <FooterWrapper>
                        <InfoText>Don't have an account?</InfoText>
                        <CreateAccount onClick={handleCreateAccount}>Create account</CreateAccount>
                    </FooterWrapper>
                </BottomBar>
            </LoginBoardWrapper>
        </FullScreenWrapper>
    );
    return (
        <>
            {modal}
        </>
    );
};

export default ConnectAccount;