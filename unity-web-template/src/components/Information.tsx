import React from 'react'
import styled from 'styled-components'

import '@assets/fonts/fonts.css';
import button from '@assets/images/webgame polygon/button_link.png'
import homepageIcon from '@assets/images/webgame polygon/icon_homepage.png'
import marketIcon from '@assets/images/webgame polygon/icon_market.png'
import dappsIcon from '@assets/images/webgame polygon/icon_dapps.png'
import {Row} from "antd";

type InformationT = {
    sold?: number;
};

const Wrapper = styled.div``

const IconContainer = styled.div<{ $width?: number }>`
    width: ${props => props.$width || 229}px;
    height: 66px;
    margin-left: 5px;
    margin-right: 5px;
    cursor: pointer;
    background-image: url(${button});
    overflow: visible;
    display: flex;
    align-items: center;
    justify-content: start;
`

const IconImage = styled.img`
    width: 40px;
    margin-left: 15px;
    margin-top: -15px;
    max-width: 100%;
    max-height: 100%;
    position: relative;
    left: 10px;
`

const IconText = styled.span`
    margin-top: -15px;
    margin-left: -15px;
    font-size: 25px;
    color: white;
    text-shadow: -2px -2px 0 #8d49b8,
    2px -2px 0 #8d49b8,
    -2px 2px 0 #8d49b8,
    2px 2px 0 #8d49b8;
    flex: 1;
    text-align: center;
    font-family: ${'GameFont'};
`

const Information: React.FC<InformationT> = () => {
    const goToHomepage = () => {
        window.open('https://bombcrypto.io')
    }

    const goToMarket = () => {
        window.open('https://market.bombcrypto.io/')
    }

    const goToDapps = () => {
        window.open('https://dapps.bombcrypto.io/bridge')
    }

    return (
        <Wrapper>
            <Row  justify='center'>
                <IconContainer onClick={goToHomepage}>
                    <IconImage src={homepageIcon} alt=""/>
                    <IconText>Homepage</IconText>
                </IconContainer>
                <IconContainer onClick={goToMarket}>
                    <IconImage src={marketIcon} alt=""/>
                    <IconText>Market</IconText>
                </IconContainer>
                <IconContainer onClick={goToDapps}>
                    <IconImage src={dappsIcon} alt=""/>
                    <IconText>Dapps</IconText>
                </IconContainer>
            </Row>
        </Wrapper>
    )
}

export default Information
