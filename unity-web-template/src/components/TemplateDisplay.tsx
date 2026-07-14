import information_bg from "@assets/images/webgame polygon/board_info.png";
import information_title from "@assets/images/webgame polygon/title_info.png";
import ContentOverlay from "./ContentOverlay.tsx";
import styled from "styled-components";
import Information from "./Information.tsx";
import {useContext} from "react";
import {useSize} from "ahooks";
import {StyleContext} from "./StyleContext.tsx";

// One consolidated board holding every footer control (links + mode switch +
// RPC + mobile QRs). Its native width scales down as a unit to fit narrow
// viewports so the art keeps its proportions — no internal reflow or scrollbar.
const BOARD_WIDTH = 1100;
const ROW_WIDTH = BOARD_WIDTH + 20;
const ROW_HEIGHT = 114;
const SIDE_PADDING = 24;

export default function TemplateDisplay() {
    const styleContext = useContext(StyleContext);

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const {fullratio} = styleContext;
    const viewport = useSize(document.documentElement);
    const available = (viewport?.width ?? ROW_WIDTH) - SIDE_PADDING;
    const scale = Math.min(1, available / ROW_WIDTH);

    return (
        <div
            style={{
                display: fullratio ? "none" : "flex",
                justifyContent: "center",
                width: "100%",
                marginTop: "50px",
            }}
        >
        {/* Sizer reserves only the scaled footprint so the page never gets a
            horizontal scrollbar; the fixed-width row is scaled from its top-left
            corner to fill it. overflow stays visible so panel titles (which sit
            above each board via negative offsets) aren't clipped. */}
        <div
            style={{
                flex: "0 0 auto",
                width: ROW_WIDTH * scale,
                height: ROW_HEIGHT * scale,
            }}
        >
        <div
            style={{
                width: ROW_WIDTH,
                display: "flex",
                justifyContent: "center",
                columnGap: "10px",
                transform: `scale(${scale})`,
                transformOrigin: "top left",
            }}
        >
            <ContentOverlay
                scrollContent={false}
                w={BOARD_WIDTH}
                h={114}
                p={"25px 10px 0px 10px"}
                bg={information_bg}
                title={
                    <StyledTitle>
                        <img src={information_title} width={200} alt={""}/>
                    </StyledTitle>
                }
            >
                <Information/>
            </ContentOverlay>
        </div>
        </div>
        </div>
    );
};

const StyledTitle = styled.div`
    font-size: 24px;
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    margin-top: -25px;
    pointer-events: none;
`;