import information_bg from "@assets/images/webgame polygon/board_info.png";
import mobile_bg from "@assets/images/webgame polygon/board_mobile.png";
import information_title from "@assets/images/webgame polygon/title_info.png";
import rpc_title from "@assets/images/webgame polygon/title_rpc.png";
import mobile_title from "@assets/images/webgame polygon/title_mobile.png";
import ContentOverlay from "./ContentOverlay.tsx";
import styled from "styled-components";
import Information from "./Information.tsx";
import {useContext} from "react";
import Mobile from "./Mobile.tsx";
import {StyleContext} from "./StyleContext.tsx";
import CustomRPC from "./CustomRPC.tsx";

export default function TemplateDisplay() {
    const styleContext = useContext(StyleContext);

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const {fullratio} = styleContext;

    return (
        <div
            style={{
                display: fullratio  ? "none" : "flex" ,
                justifyContent: "center",
                columnGap: "10px",
                marginTop: "50px",
            }}
        >
            <ContentOverlay
                scrollContent={false}
                w={746}
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
            <ContentOverlay
                scrollContent={false}
                w={270}
                h={114}
                p={"25px 10px 0px 15px"}
                bg={mobile_bg}
                title={
                    <StyledTitle>
                        <img src={rpc_title} width={200} alt={"Custom your RPC"}/>
                    </StyledTitle>
                }
            >
                <CustomRPC/>
            </ContentOverlay>
            <ContentOverlay
                scrollContent={false}
                w={227}
                h={114}
                p={"10px 10px 10px 10px"}
                bg={mobile_bg}
                title={
                    <StyledTitle>
                        <img src={mobile_title} width={200} alt={""}/>
                    </StyledTitle>
                }
            >
                <Mobile/>
            </ContentOverlay>
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