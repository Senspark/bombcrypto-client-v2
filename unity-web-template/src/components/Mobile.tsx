import React, {  } from 'react'
import styled from 'styled-components'

import iosQR from '@assets/images/webgame polygon/qr_IOS.png'
import androidQR from '@assets/images/webgame polygon/qr_Android.png'
import appleIcon from '@assets/images/webgame polygon/icon_apple.png'
import chPlayIcon from '@assets/images/webgame polygon/icon_googlePlay.png'
import {Row} from "antd";

type MobileT = {
    sold?: number;
};

const Wrapper = styled.div``

const QRIcon = styled.img`
  width: 100%;
  cursor: pointer;
`
const SmallIcon = styled.img`
    width: 30%;
    position: absolute;
    bottom: 40%;
    right: -15%;
    cursor: pointer;

`

const IconWrapper = styled.div`
    flex: 1;
    margin: 0 10px;
    position: relative;
    max-width: calc(48% - 20px);
`

const Mobile: React.FC<MobileT> = () => {
    const goToIosQR = () => {
        window.open('https://apps.apple.com/us/app/bomber-land-battle-pvp-game/id1673632517')
    }

    const goToAndroidQR = () => {
        window.open('https://play.google.com/store/apps/details?id=com.senspark.bomber.land.boom.battle.bombgames')
    }

    return (
        <Wrapper>
            <Row justify='start' align='middle'>
                <IconWrapper>
                    <QRIcon onClick={goToIosQR} src={iosQR} alt=""/>
                    <SmallIcon src={appleIcon} alt=""/>
                </IconWrapper>
                <IconWrapper>
                    <QRIcon onClick={goToAndroidQR} src={androidQR} alt=""/>
                    <SmallIcon src={chPlayIcon} alt=""/>
                </IconWrapper>
            </Row>
        </Wrapper>
    )
}

export default Mobile
