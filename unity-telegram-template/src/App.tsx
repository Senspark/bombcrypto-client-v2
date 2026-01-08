import './App.css';

// React imports
import { useEffect, useState, useMemo } from 'react';

// Third-party libraries
import { ConfigProvider, Layout, theme } from 'antd';
import { useAtomValue } from 'jotai';
import { useTonConnectUI } from "@tonconnect/ui-react"; // Add this import

// Components
import { UnityRenderer } from './components/UnityRenderer.tsx';
import { ConnectWallet } from './components/ConnectWallet.tsx';
import BlockIpDisplay from './components/BlockIpDisplay.tsx';
import { AppNotification } from './components/AppNotification.tsx';
import MaintenanceDisplay from './components/MaintenanceDisplay.tsx';
import AppBackground from './components/AppBackground.tsx';
import { StyleProvider } from './components/StyleContext.tsx';
import AppFooter from './components/AppFooter.tsx';

// Services and utilities
import { authApi, ipService, logger, tonService, unityCom, unityService } from './services/GlobalServices.ts';
import { EnvConfig } from './services/EnvConfig.ts';
import { serverMaintenanceStatus } from './services/CheckServerService.ts';

const { Content } = Layout;

/**
 * Type definitions
 */
type IpCheckState = 'checking' | 'not-allow' | 'allow';

/**
 * Custom hook for handling IP check logic
 */
const useIpCheck = (): IpCheckState => {
  const ignoreIpCheck = EnvConfig.ignoreIpCheck();
  const [ipAllowed, setIpAllowed] = useState<IpCheckState>(
    ignoreIpCheck ? 'allow' : 'checking'
  );

  useEffect(() => {
    if (!ignoreIpCheck) {
      const checkIpStatus = async () => {
        try {
          const allowed = await ipService.checkIp();
          if (!allowed) {
            logger.error("IP is not allowed");
          }
          setIpAllowed(allowed ? 'allow' : 'not-allow');
        } catch (error) {
          logger.error(`Error checking IP: ${error}`);
          setIpAllowed('not-allow');
        }
      };
      
      checkIpStatus();
    }
  }, [ignoreIpCheck]);

  return ipAllowed;
};

/**
 * Custom hook for handling wallet connection
 */
const useWalletConnection = () => {
  const [connected, setConnected] = useState<boolean>(
    authApi.getAccountTon().jwt != null
  );
  const [showConnectWallet, setShowConnectWallet] = useState<boolean>(false);
  
  const handleConnectionChange = (isConnected: boolean): void => {
    setConnected(isConnected);
    if (isConnected) {
      setShowConnectWallet(false);
    }
  };
  
  const handleDisconnect = async (): Promise<void> => {
    try {
      await unityCom.logout();
      setConnected(false);
    } catch (error) {
      logger.error(`Error during disconnect: ${error}`);
    }
  };
  
  const handleConnectButtonClick = (): void => {
    setShowConnectWallet(false); // force unmount
    setTimeout(() => setShowConnectWallet(true), 0); // re-mount next tick
  };
  
  return {
    connected,
    showConnectWallet,
    handleConnectionChange,
    handleDisconnect,
    handleConnectButtonClick
  };
};

/**
 * Main App component
 */
const App = () => {
  // Custom hooks for state management
  const ipAllowed = useIpCheck();
  const maintenanceStatus = useAtomValue(serverMaintenanceStatus);
  const {
    connected,
    showConnectWallet,
    handleConnectionChange,
    handleDisconnect,
    handleConnectButtonClick
  } = useWalletConnection();
  
  // Initialize TonConnect on every page load
  const [tonConnectUI] = useTonConnectUI();
  useEffect(() => {
    // Set the TonConnect instance in the TonService
    tonService.setTonConnect(tonConnectUI);
  }, [tonConnectUI]);

  // Handle maintenance mode
  useEffect(() => {
    if (maintenanceStatus === 'maintenance' && connected) {
      logger.log('Server is in maintenance mode. Quit Unity...');
      unityService.quitUnity();
    }
  }, [maintenanceStatus, connected]);

  /**
   * Renders the appropriate display based on app state
   * Flow: Check IP → Check maintenance → Show Unity renderer
   */
  const renderDisplay = useMemo((): JSX.Element | null => {
    // IP check failed
    if (ipAllowed === 'not-allow') {
      return <BlockIpDisplay />;
    } 
    
    // IP allowed, check maintenance status
    if (ipAllowed === 'allow') {
      if (maintenanceStatus === 'maintenance') {
        return <MaintenanceDisplay />;
      }
      
      if (maintenanceStatus === 'normal') {
        return (
          <div style={containerStyle}>
            <div style={rendererContainerStyle}>
              <UnityRenderer />
            </div>
          </div>
        );
      }
    }
    
    // Still checking IP or other undefined state
    return null;
  }, [ipAllowed, maintenanceStatus]);

  return (
    <ConfigProvider theme={{ algorithm: theme.darkAlgorithm }}>
      <Layout style={layoutStyle}>
        <StyleProvider>
          <Content style={contentStyle}>
            <AppBackground>
              {renderDisplay}
            </AppBackground>
          </Content>
          <AppFooter 
            connected={connected} 
            onDisconnect={handleDisconnect} 
            onConnectClick={handleConnectButtonClick} 
          />
        </StyleProvider>
        {showConnectWallet && <ConnectWallet setConnected={handleConnectionChange} />}
      </Layout>
      <AppNotification />
    </ConfigProvider>
  );
};


const contentStyle: React.CSSProperties = {
    textAlign: 'center',
    minHeight: 120,
    lineHeight: '120px',
};

const layoutStyle = {
    borderRadius: 8,
    overflow: 'hidden',
    height: '100vh',
};

const containerStyle: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
};

const rendererContainerStyle: React.CSSProperties = {
  flex: '1 0 auto',
};

export default App;
