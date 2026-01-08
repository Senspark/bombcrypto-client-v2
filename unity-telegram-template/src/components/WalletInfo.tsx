import { useState, useEffect } from 'react';
import { authApi, tonService } from "../services/GlobalServices";
import './WalletInfo.css';
import NotificationService from "../services/NotificationService.ts";

import icon_ton from "@assets/icon_ton.png";
import icon_copy from "@assets/icon_copy.png";

type WalletInfoProps = {
    onDisconnect: () => void;
}


export const WalletInfo = ({ onDisconnect }: WalletInfoProps) => {
    const notificationService = new NotificationService();
    const [walletAddress, setWalletAddress] = useState<string>('');
    const [fullAddress, setFullAddress] = useState<string>('');
    const [balance, setBalance] = useState<number>(0);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchWalletInfo = async () => {
            setIsLoading(true);
            
            // Get wallet address
            const address = authApi.getFriendlyWalletAddress();
            if (address) {
                setFullAddress(address);
                // Format address to show only first 6 and last 6 characters
                const formattedAddress = `${address.substring(0, 6)}...${address.substring(address.length - 6)}`;
                setWalletAddress(formattedAddress);
            }
            
            // Get wallet balance
            try {
                const balance = await tonService.getTonBalance();
                setBalance(balance);
            } catch (error) {
                console.error('Failed to fetch balance:', error);
            }
            
            setIsLoading(false);
        };
        
        fetchWalletInfo();
        
        // Refresh balance every 30 seconds
        const interval = setInterval(async () => {
            try {
                const balance = await tonService.getTonBalance();
                setBalance(balance);
            } catch (error) {
                console.error('Failed to update balance:', error);
            }
        }, 30000);
        
        return () => clearInterval(interval);
    }, []);
    
    return (
        <div className="wallet-info">
            {isLoading ? (
                <div className="wallet-loading">Loading...</div>
            ) : (
                <>
                    <img src={icon_ton} width="25" height="25" />
                    <div className="wallet-data">
                        <div className="wallet-info-row">
                            <span className="wallet-balance">{balance.toFixed(4)} TON</span>
                            <button
                                className="wallet-copy-button"
                                onClick={() => {
                                    navigator.clipboard.writeText(fullAddress);
                                    notificationService.show("Address copied", ``, 2, 'success');
                                }}
                            >
                                <span className="wallet-address">{walletAddress}</span>
                                <img src={icon_copy} width="15" height="15" alt="Copy" />
                            </button>
                        </div>
                    </div>
                    <button className="disconnect-button" onClick={onDisconnect}>
                        Disconnect
                    </button>
                </>
            )}
        </div>
    );
};
