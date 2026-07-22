import React from 'react';
import { useAtom } from 'jotai';
import { useAppKit, useAppKitAccount, AppKitNetworkButton } from '@reown/appkit/react';
import { appKitButtonAtom } from './AppKitButtonAtom';
import { unityCommunicator } from '../hooks/GlobalServices';

type AppKitButtonProps = object;

const styles = {
    connectButtonFake: {
        backgroundColor: '#5773ff',
        color: 'white',
        border: 'none',
        borderRadius: '25px',
        cursor: 'pointer',
        fontSize: '16px',
        transition: 'background-color 0.2s ease',
        height: '40px',
        width: '145px',
    } as React.CSSProperties,

    connectButton: {
        backgroundColor: '#5773ff',
        color: '#ffffff',
        border: '1px solid var(--wui-color-gray-glass-010)',
        borderRadius: 'var(--wui-border-radius-m)',
        padding: '10px 16px',
        minHeight: '40px',
        fontSize: '16px',
        fontWeight: 500,
        cursor: 'pointer',
        transition: 'background-color 0.2s ease',
    } as React.CSSProperties,
};

const truncateAddress = (addr: string): string =>
    addr.length > 10 ? `${addr.slice(0, 6)}...${addr.slice(-4)}` : addr;

const AppKitButton: React.FC<AppKitButtonProps> = () => {
    const [state] = useAtom(appKitButtonAtom);
    const { open } = useAppKit();
    const { address, isConnected } = useAppKitAccount();

    const handleConnectWallet = async () => {
        try {
            await unityCommunicator.continueConnection(true);
            window.setUseAccount(false);
        } catch (error) {
            console.error('Error connecting wallet:', error);
        }
    };

    if (state.showFakeConnect) {
        return (
            <>
                <AppKitNetworkButton />
                <button
                    style={styles.connectButtonFake}
                    onMouseOver={(e) => { e.currentTarget.style.backgroundColor = '#3a5dca'; }}
                    onMouseOut={(e) => { e.currentTarget.style.backgroundColor = '#5773ff'; }}
                    onClick={handleConnectWallet}
                >
                    Connect Wallet
                </button>
            </>
        );
    }

    const connected = isConnected && !!address;
    const label = connected ? truncateAddress(address!) : 'Connect Wallet';

    return (
        <>
            <AppKitNetworkButton />
            <button
                style={styles.connectButton}
                onMouseOver={(e) => { e.currentTarget.style.backgroundColor = '#3a5dca'; }}
                onMouseOut={(e) => { e.currentTarget.style.backgroundColor = '#5773ff'; }}
                onClick={() => open(connected ? { view: 'Account' } : undefined)}
            >
                {label}
            </button>
        </>
    );
};

export default AppKitButton;
