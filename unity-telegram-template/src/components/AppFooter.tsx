import { useContext } from "react";
import { StyleContext } from "./StyleContext";
import { WalletInfo } from "./WalletInfo";

import './WalletInfo.css';

interface AppFooterProps {
  connected: boolean;
  onDisconnect: () => void;
  onConnectClick: () => void;
}

export default function AppFooter({ connected, onDisconnect, onConnectClick }: AppFooterProps) {
  const styleContext = useContext(StyleContext);

  if (!styleContext) {
      throw new Error("StyleContext must be used within a StyleProvider");
  }

  const { fullRatio } = styleContext;
  if (fullRatio) {
      return null;
  }
  
  // Otherwise render the footer with wallet connection UI
  return (
    <div className="bottom-bar">
      {connected ? (
        <WalletInfo onDisconnect={onDisconnect} />
      ) : (
        <button className="connect-button" onClick={onConnectClick}>
          Connect Wallet
        </button>
      )}
    </div>
  );
};
