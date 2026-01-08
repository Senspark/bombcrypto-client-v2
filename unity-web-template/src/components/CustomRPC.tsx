import React from 'react'
import styled from 'styled-components'

import '@assets/fonts/fonts.css';
import button from '@assets/images/webgame polygon/button_link.png'
import customrpcIcon from '@assets/images/webgame polygon/icon_customrpc.png'
import {Row} from "antd";
import Metamask from "./Metamask.tsx";
import {useState} from "react";

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

const CustomRPC: React.FC<InformationT> = () => {
    const [openMetamask, setOpenMetamask] = useState(false);

    const handleMetamaskClicked = () => {
        setOpenMetamask(true);
    }

    return (
        <Wrapper>
            <Row>
                <IconContainer onClick={handleMetamaskClicked}>
                    <IconImage src={customrpcIcon} alt=""/>
                    <IconText>RPC urls</IconText>
                </IconContainer>
            </Row>
            <Metamask open={openMetamask} setOpen={setOpenMetamask}/>
        </Wrapper>
    )
}

export default CustomRPC
