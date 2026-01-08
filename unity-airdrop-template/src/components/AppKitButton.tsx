import React, { useState } from 'react';
import { useAtom } from 'jotai';
import { appKitButtonAtom } from './AppKitButtonAtom';
import { FaRegCopy, FaCheck } from 'react-icons/fa';

type AppKitButtonProps = object;

// Styles organized by component
const styles = {
    container: {
        padding: '12px 16px',
        border: '1px solid #2d3748',
        borderRadius: '12px',
        color: '#e2e8f0',
        display: 'flex',
        flexDirection: 'column',
        gap: '8px',
        maxWidth: '100%',
        minWidth: '200px',
        maxHeight: '60px',
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        fontSize: '14px',
        lineHeight: '1.4',
    } as React.CSSProperties,

    row: {
        display: 'flex',
        alignItems: 'center',
        gap: '8px',
        overflow: 'hidden',
    } as React.CSSProperties,

    label: {
        color: '#a0aec0',
        fontSize: '12px',
        fontWeight: 600,
        textTransform: 'uppercase',
        letterSpacing: '0.5px',
        flexShrink: 0,
    } as React.CSSProperties,

    networkValue: {
        color: '#48bb78',
        fontWeight: 500,
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
    } as React.CSSProperties,

    addressValue: {
        color: '#63b3ed',
        fontWeight: 500,
        fontFamily: 'Monaco, "Courier New", monospace',
        fontSize: '13px',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
    } as React.CSSProperties,

    addressClickable: {
        color: '#63b3ed',
        fontWeight: 500,
        fontFamily: 'Monaco, "Courier New", monospace',
        fontSize: '13px',
        overflow: 'hidden',
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
        cursor: 'pointer',
        position: 'relative',
        transition: 'color 0.2s',
        display: 'inline-flex',
        alignItems: 'center',
        gap: 4,
    } as React.CSSProperties,

    icon: {
        marginLeft: 6,
        fontSize: 14,
        transition: 'opacity 0.2s',
    } as React.CSSProperties,

    copiedIcon: {
        marginLeft: 6,
        fontSize: 14,
        transition: 'opacity 0.2s',
        color: '#38a169',
    } as React.CSSProperties,

    copyIcon: {
        marginLeft: 6,
        fontSize: 14,
        transition: 'opacity 0.2s',
        color: '#a0aec0',
    } as React.CSSProperties,
};

// Simple UI components
const Container: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <div style={styles.container}>{children}</div>
);

const Row: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <div style={styles.row}>{children}</div>
);

const Label: React.FC<{ children: React.ReactNode }> = ({ children }) => (
    <span style={styles.label}>{children}</span>
);

// Network display component
const NetworkInfo: React.FC<{ network: string }> = ({ network }) => (
    <Row>
        <Label>Network:</Label>
        <span style={styles.networkValue}>{network || 'Unknown'}</span>
    </Row>
);

// Address display component with copy functionality
const AddressInfo: React.FC<{
    address: string;
    onCopy: () => void;
    copied: boolean;
}> = ({ address, onCopy, copied }) => (
    <Row>
        <Label>Address:</Label>
        <span
            style={address ? styles.addressClickable : styles.addressValue}
            onClick={onCopy}
            title={address ? 'Click to copy' : ''}
        >
      {address || 'Unknown'}
            {address && (
                copied ? (
                    <FaCheck style={styles.copiedIcon} />
                ) : (
                    <FaRegCopy style={styles.copyIcon} />
                )
            )}
    </span>
    </Row>
);

const AppKitButton: React.FC<AppKitButtonProps> = () => {
    const [state] = useAtom(appKitButtonAtom);
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        if (state.address) {
            await navigator.clipboard.writeText(state.address);
            setCopied(true);
            setTimeout(() => setCopied(false), 1200);
        }
    };

    if (state.showCustomUI) {
        return (
            <Container>
                <NetworkInfo network={state.network ?? "Unknown"} />
                <AddressInfo
                    address={state.address ?? "Unknown"}
                    onCopy={handleCopy}
                    copied={copied}
                />
            </Container>
        );
    }

    return <appkit-button />;
};

export default AppKitButton;