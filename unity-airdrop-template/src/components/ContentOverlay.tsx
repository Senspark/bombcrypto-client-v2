import React from 'react';
import styled from 'styled-components';

type Props = {
    w: number;
    h: number;
    p: string;
    bg: string;
    title?: React.ReactNode;
    scrollContent?: boolean
    children: React.ReactNode;
}

const ContentOverlay: React.FC<Props> = ({ scrollContent = true, bg, children, title, ...rest }) => {
    return (
        <Wrapper {...rest} style={{ backgroundImage: `url(${bg})`, backgroundRepeat: 'no-repeat', backgroundSize: '100% 100%', backgroundPosition: 'top' }}>
    <div>
        <div className="title">{title}</div>
        <div className={`content ${!scrollContent && 'no-scroll'}`}>{children}</div>
    </div>
    </Wrapper>
    )
}

export default ContentOverlay;

const Wrapper = styled.div<{ w: number, h: number, p: string }>`
    width: ${({ w }) => w}px;
    height: ${({ h }) => h}px;
    padding: ${({ p }) => p};
    position: relative;
  
    >div {
      .title {
        position: absolute;
        top: -15px;
        left: 0;
        text-align: center;
        width: 100%;
      }
      
      .content {
        max-height: 110px;
        overflow-y: scroll;
      }
      .no-scroll {
        overflow-y: hidden !important;
      }
    }
`
