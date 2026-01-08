import {ConfigProvider, Layout, theme} from "antd";
import {EnvConfig} from "./configs/EnvConfig.ts";
import React, {useEffect, useState} from "react";
import AppFooter from "./components/AppFooter.tsx";
import AppBackground from "./components/AppBackground.tsx";
import GameDisplay from "./components/GameDisplay.tsx";
import {AppNotification} from "./components/AppNotification.tsx";
import {useAppService} from "./hooks/AppStarterHook.tsx";
import {checkIpService, logger, unityService} from "./hooks/GlobalServices.ts";
import BlockIpDisplay from "./components/BlockIpDisplay.tsx";
import {useAtomValue} from "jotai";
import {serverMaintenanceStatus} from "./controllers/CheckServerService.ts";
import MaintenanceDisplay from "./components/MaintenanceDisplay.tsx";
import {StyleProvider} from "./components/StyleContext.tsx";

const {Content} = Layout;

export default function App() {
    useAppService();
    const ignoreIpCheck = EnvConfig.ignoreIpCheck();
    const [ipAllowed, setIpAllowed] = useState<IpCheckState>(ignoreIpCheck ? 'allow' : 'checking');
    const maintenanceStatus = useAtomValue(serverMaintenanceStatus);


    useEffect(() => {
        if (!ignoreIpCheck) {
            checkIpService.checkIp().then((allow) => {
                if (!allow) {
                    logger.error("IP is not allowed");
                }
                setIpAllowed(allow ? 'allow' : 'not-allow');
            });
        }
    }, []);

    useEffect(() => {
        if (maintenanceStatus === 'maintenance') {
            logger.log('Server is in maintenance mode. Quit Unity...');
            unityService.quitUnity();
        }
    }, [maintenanceStatus]);

    // Check ip => check server maintenance => check connected => unity renderer
    const renderDisplay = (): JSX.Element | null => {
        // Check ip
        if (ipAllowed === 'not-allow') {
            return <BlockIpDisplay/>;
        } else if (ipAllowed === 'allow') {
            console.log("Maintenance status: ", maintenanceStatus);
            // Check server maintenance
            if (maintenanceStatus === 'maintenance') {
                return <MaintenanceDisplay/>;
            } else if (maintenanceStatus === 'normal') {
                // Check connected
                return (
                    <>
                        <div style={{display: 'flex', flexDirection: 'column', height: '100%'}}>
                            <div style={{flex: '1 0 auto'}}>
                                <GameDisplay/>
                            </div>
                        </div>
                    </>

                )
            }
        }
        return null;
    };
    
    return (
        <ConfigProvider theme={{algorithm: theme.darkAlgorithm}}>
            <Layout style={layoutStyle}>
                <StyleProvider>
                    <Content style={contentStyle}>
                        <AppBackground>
                            {renderDisplay()}
                        </AppBackground>
                    </Content>
                    <AppFooter/>
                </StyleProvider>
            </Layout>
            <AppNotification/>
        </ConfigProvider>
    )
}

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

type IpCheckState = 'checking' | 'not-allow' | 'allow';
