import React, { useEffect } from 'react';
import { useAtom } from 'jotai';
import { useAppKit, useAppKitAccount, AppKitNetworkButton } from '@reown/appkit/react';
import { appKitButtonAtom } from './AppKitButtonAtom';
import { unityCommunicator } from '../hooks/GlobalServices';
import LoginModal from './LoginModal';

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

    networkLabel: {
        display: 'inline-flex',
        alignItems: 'center',
        backgroundColor: 'rgba(255, 255, 255, 0.1)',
        color: '#ffffff',
        border: '1px solid var(--wui-color-gray-glass-010)',
        borderRadius: 'var(--wui-border-radius-m)',
        padding: '0 16px',
        height: '40px',
        fontSize: '16px',
        fontWeight: 500,
        whiteSpace: 'nowrap',
    } as React.CSSProperties,
};

const truncateAddress = (addr: string): string =>
    addr.length > 10 ? `${addr.slice(0, 6)}...${addr.slice(-4)}` : addr;

const AppKitButton: React.FC<AppKitButtonProps> = () => {
    const [state] = useAtom(appKitButtonAtom);
    const { open, close } = useAppKit();
    const { address, isConnected } = useAppKitAccount();

    const handleConnectWallet = async () => {
        try {
            await unityCommunicator.continueConnection(true);
            window.setUseAccount(false);
        } catch (error) {
            console.error('Error connecting wallet:', error);
        }
    };

    // AppKit's picker ignores a click on the network the wallet is already on, so
    // that pick is handled here: the popup takes the live wallet instead of the saved one
    const hasSaved = !!state.saved;
    useEffect(() => {
        if (!hasSaved) return;
        const onClick = (e: MouseEvent) => {
            const picked = e.composedPath().find(n => (n as HTMLElement).tagName === 'WUI-LIST-NETWORK');
            if (!picked || !(picked as HTMLElement & { selected?: boolean }).selected) return;
            LoginModal.syncToLiveWallet().then(() => close());
        };
        document.addEventListener('click', onClick, true);
        return () => document.removeEventListener('click', onClick, true);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [hasSaved]);

    if (state.showFakeConnect) {
        return (
            <>
                {/* No network button before "Connect Wallet": it would switch the wallet other tabs share */}
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

    // Logged in: show the network/wallet this tab plays with. AppKit's own
    // buttons follow the live wallet, which another tab may have switched.
    if (state.showCustomUI && state.isWallet && state.address) {
        return (
            <>
                {state.network && <span style={styles.networkLabel}>{state.network}</span>}
                <button
                    style={styles.connectButton}
                    onMouseOver={(e) => { e.currentTarget.style.backgroundColor = '#3a5dca'; }}
                    onMouseOut={(e) => { e.currentTarget.style.backgroundColor = '#5773ff'; }}
                    onClick={() => open(connected ? { view: 'Account' } : undefined)}
                >
                    {truncateAddress(state.address)}
                </button>
            </>
        );
    }

    // Login popup is asking about a saved profile: show it, not the live wallet
    const saved = connected ? state.saved : undefined;
    const label = connected ? truncateAddress(saved?.address ?? address!) : 'Connect Wallet';

    return (
        <>
            {/* Not connected: a network picked here is not what the login popup uses */}
            {connected && (saved
                ? <button style={{...styles.networkLabel, cursor: 'pointer'}} onClick={() => open({ view: 'Networks' })}>{saved.network}</button>
                : <AppKitNetworkButton />)}
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
