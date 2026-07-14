import React, {useState} from 'react'
import styled from 'styled-components'
import {Row} from "antd"

import '@assets/fonts/fonts.css';
import button from '@assets/images/webgame polygon/button_link.png'
import homepageIcon from '@assets/images/webgame polygon/icon_homepage.png'
import marketIcon from '@assets/images/webgame polygon/icon_market.png'
import dappsIcon from '@assets/images/webgame polygon/icon_dapps.png'
import customrpcIcon from '@assets/images/webgame polygon/icon_customrpc.png'
import {QRCodeCanvas} from 'qrcode.react'
import Metamask from "./Metamask.tsx";
import {getCurrentLanding, openLandingMode} from "../controllers/LandingUtils.ts";

const ANDROID_STORE_URL = 'https://play.google.com/store/apps/details?id=com.senspark.bomber.land.boom.battle.bombgames';

type InformationT = {
    sold?: number;
};

type LinkItem = {
    key: string;
    label: string;
    icon?: string;
    width?: number;
    onClick: () => void;
};

const BUTTON_WIDTH = 150;
const BUTTON_MARGIN = 4;

const Wrapper = styled.div`
    width: 100%;
`

const IconContainer = styled.div<{ $width?: number }>`
    width: ${props => props.$width || BUTTON_WIDTH}px;
    height: 66px;
    margin-left: ${BUTTON_MARGIN}px;
    margin-right: ${BUTTON_MARGIN}px;
    cursor: pointer;
    flex-shrink: 0;
    background-image: url(${button});
    background-size: 100% 100%;
    background-repeat: no-repeat;
    overflow: visible;
    display: flex;
    align-items: center;
    justify-content: start;
`

const IconImage = styled.img`
    width: 28px;
    margin-left: 10px;
    margin-top: -15px;
    max-width: 100%;
    max-height: 100%;
    position: relative;
    left: 6px;
`

const IconText = styled.span`
    margin-top: -15px;
    margin-left: -10px;
    font-size: 15px;
    color: white;
    text-shadow: -2px -2px 0 #8d49b8,
    2px -2px 0 #8d49b8,
    -2px 2px 0 #8d49b8,
    2px 2px 0 #8d49b8;
    flex: 1;
    text-align: center;
    font-family: ${'GameFont'};
`

const QrContainer = styled.div`
    width: 70px;
    height: 70px;
    margin: 0 8px;
    flex-shrink: 0;
`

const Information: React.FC<InformationT> = () => {
    const [openMetamask, setOpenMetamask] = useState(false);
    const otherMode = getCurrentLanding() === 'treasure' ? 'adventure' : 'treasure';

    const links: LinkItem[] = [
        {key: 'homepage', label: 'Homepage', icon: homepageIcon, onClick: () => window.open('https://bombcrypto.io')},
        {key: 'market', label: 'Market', icon: marketIcon, onClick: () => window.open('https://market.bombcrypto.io/')},
        {key: 'dapps', label: 'Dapps', icon: dappsIcon, onClick: () => window.open('https://dapps.bombcrypto.io/bridge')},
        {key: 'leaderboard', label: 'Leaderboard', onClick: () => window.open('https://treasure-mode.bombcrypto.io/')},
        {
            key: 'switchMode',
            label: otherMode === 'treasure' ? 'Treasure Mode' : 'Adventure/PvP',
            width: 190,
            onClick: () => openLandingMode(otherMode),
        },
        {key: 'rpc', label: 'RPC urls', icon: customrpcIcon, onClick: () => setOpenMetamask(true)},
    ];

    return (
        <Wrapper>
            <Row justify='center' align='middle' wrap={false}>
                {links.map(link => (
                    <IconContainer key={link.key} $width={link.width} onClick={link.onClick}>
                        {link.icon && <IconImage src={link.icon} alt=""/>}
                        <IconText>{link.label}</IconText>
                    </IconContainer>
                ))}
                <QrContainer>
                    <QRCodeCanvas
                        value={ANDROID_STORE_URL}
                        size={70}
                        style={{cursor: 'pointer'}}
                        onClick={() => window.open(ANDROID_STORE_URL)}
                    />
                </QrContainer>
            </Row>
            <Metamask open={openMetamask} setOpen={setOpenMetamask}/>
        </Wrapper>
    )
}

export default Information
