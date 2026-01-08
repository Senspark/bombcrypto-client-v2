import React, {useState, useEffect} from 'react';
import {Modal, Button} from 'antd';
import {getRpc} from "../controllers/RpcNetworkUtils.ts";
import {walletService} from "../hooks/GlobalServices.ts";
import {WalletIdUtils} from "../controllers/WalletIdUtils.ts";
import {useAppService} from "../hooks/AppStarterHook.tsx";

// Singleton state for modal control
const modalState: {
    visible: boolean;
    address: string;
    network: string;
    hasOldData: boolean;
    resetClicked: boolean;
    isProd?: boolean;
    resolve?: (result: { address: string; network: string; hasChange: boolean }) => void;
    hasChange?: boolean;
} = {
    visible: false,
    address: '',
    network: '',
    hasOldData: false,
    resetClicked: false,
    isProd: undefined,
    hasChange: false,
};

let rerender: (() => void) | null = null;

function triggerRerender() {
    if (rerender) rerender();
}

const LoginModal: React.FC = () => {
    const [, setVersion] = useState(0);
    const [modalContainer, setModalContainer] = useState<HTMLDivElement | null>(null);
    const [showResetSuccess, setShowResetSuccess] = useState(false); // NEW STATE
    const { close } = useAppService();

    useEffect(() => {
        rerender = () => setVersion(v => v + 1);
        return () => {
            rerender = null;
        };
    }, []);

    const [isWalletConnected, setIsWalletConnected] = useState(walletService.getConnection());

    // Poll wallet connection status every 1s
    useEffect(() => {
        const interval = setInterval(() => {
            const connected = walletService.getConnection();
            setIsWalletConnected(prev => {
                if (prev !== connected) return connected;
                return prev;
            });
        }, 200);
        return () => clearInterval(interval);
    }, []);


    useEffect(() => {
        // Create container only once
        const container = document.createElement('div');
        container.style.pointerEvents = 'none';
        document.body.appendChild(container);
        setModalContainer(container);

        return () => {
            if (container.parentNode) {
                container.parentNode.removeChild(container);
            }
        };
    }, []);

    // Dynamic title based on connection
    const modalTitle = isWalletConnected
        ? "Do you want to use this profile to login?"
        : <span style={{ color: 'red' }}>Please connect your wallet before login</span>;


    const handleOk = () => {
        modalState.visible = false;
        triggerRerender();
        if (modalState.resolve) {
            modalState.resolve({
                address: modalState.address,
                network: modalState.network,
                hasChange: !!modalState.hasChange
            });
            modalState.resolve = undefined;
        }
        modalState.resetClicked = false;
        modalState.hasChange = false;
        close();
    };


    const handleReset = async () => {
        const chainId = await walletService.getCurrentNetworkFromMetaMask();
        if (chainId) {
            const network = getRpc(walletService.currentNetworkTypeFromChainId(chainId)!, modalState.isProd ?? true);
            modalState.network = network?.chainName || '';
        } else {
            modalState.network = INVALID_NETWORK_MESSAGE;
        }

        const walletAddress = await walletService.getWalletAddress();
        modalState.address = walletAddress || '';

        modalState.resetClicked = true;
        modalState.hasChange = true;
        WalletIdUtils.removeWalletId();
        triggerRerender();
        setShowResetSuccess(true); // Show success text
        setTimeout(() => {
            setShowResetSuccess(false); // Hide after 3s
        }, 3000);
    };

    if (!modalContainer) {
        return null;
    }

    return (
        <Modal
            open={modalState.visible}
            title={modalTitle}
            onCancel={handleReset}
            style={{top: 0}}
            width={600}
            footer={
                <div style={{display: 'flex', alignItems: 'center', width: '100%'}}>
                    <span style={{
                        fontSize: 12,
                        color: '#888',
                        flex: 1,
                        minWidth: 0,
                        justifyContent: 'flex-start',
                        display: 'flex'
                    }}>
                        You can change the wallet and network you want before press OK
                    </span>
                    <div style={{display: 'flex', alignItems: 'center', gap: 8, flexShrink: 0}}>
                        {modalState.hasOldData && !showResetSuccess && !modalState.resetClicked && !modalState.hasChange && (
                            <Button key="reset" onClick={handleReset} disabled={!isWalletConnected}>
                                Reset
                            </Button>
                        )}
                        {modalState.hasOldData && showResetSuccess && (
                            <span style={{color: 'green', fontWeight: 500}}>Reset Success</span>
                        )}
                        <Button key="ok" type="primary" onClick={handleOk}
                                disabled={!modalState.address || !modalState.network || !isWalletConnected || modalState.network === INVALID_NETWORK_MESSAGE}>
                            OK
                        </Button>
                    </div>
                </div>
            }
            // centered
            mask={false}
            maskClosable={false}
            closable={false}
            getContainer={() => modalContainer}
        >
            {isWalletConnected && (
                <>
                    <div style={{marginBottom: 6}}>
                        <b>Wallet:</b> {modalState.address || <span style={{color: '#888'}}>(not set)</span>}
                    </div>
                    <div>
                        <b>Network:</b> {modalState.network === INVALID_NETWORK_MESSAGE ? <span style={{color: 'red'}}>{modalState.network}</span> : (modalState.network || <span style={{color: '#888'}}>(not set)</span>)}
                    </div>
                </>
            )}
        </Modal>
    );
};

// Define a type for the static methods
interface LoginModalStatics {
    open: (address: string, network: string, hasOldData?: boolean) => void;
    waitForUser: () => Promise<{ address: string; network: string; hasChange: boolean }>;
    setAddress: (address: string | undefined) => void;
    setNetwork: (network: string | undefined) => void;
    setStatus: (isProd: boolean) => void;
    resetSkipModal: () => void;
}

// Đảm bảo user chỉ confirm 1 lần thôi trừ khi nào user logout thì mới đc show dialog confirm này lại
let _skipModal = false;

const LoginModalStaticsImpl: LoginModalStatics = {
    open: (address, network, hasOldData = false) => {
        if (_skipModal) return;
        modalState.visible = true;
        modalState.address = address;
        modalState.network = network;
        modalState.hasOldData = hasOldData;
        modalState.resetClicked = false;
        modalState.hasChange = false;
        triggerRerender();
    },
    waitForUser: () => {
        if (_skipModal) {
            return Promise.resolve({
                address: modalState.address,
                network: modalState.network,
                hasChange: false
            });
        }
        return new Promise(resolve => {
            modalState.resolve = (result) => {
                _skipModal = true;
                resolve(result);
            };
        });
    },
    setAddress: (address) => {
        modalState.address = address || '';
        modalState.hasChange = true;
        triggerRerender();
    },
    setNetwork: (network) => {
        modalState.network = network || '';
        modalState.hasChange = true;
        triggerRerender();
    },
    setStatus: (isProd) => {
        modalState.isProd = isProd;
        triggerRerender();
    },
    resetSkipModal: () => {
        _skipModal = false;
    }
};

// Attach static methods to the component
const LoginModalWithStatics = Object.assign(LoginModal, LoginModalStaticsImpl);

export default LoginModalWithStatics as typeof LoginModal & LoginModalStatics;

// Special constant for invalid network state
const INVALID_NETWORK_MESSAGE = 'Please choose network again';
