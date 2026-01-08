import background from "../assets/background.png";
import React, {useContext} from "react";
import {StyleContext} from "./StyleContext.tsx";

type Props = {
    children: React.ReactNode;
};

export default function AppBackground(props: Props) {
    const styleContext = useContext(StyleContext);

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const { fullRatio } = styleContext;

    const bgStyle: React.CSSProperties = {
        display: "block",
        width: "100%",
        height: "100%",
        objectFit: "cover",
        backgroundImage: fullRatio ? undefined :`url(${background})`,
        backgroundRepeat: "repeat",
        zIndex: -2
    };
    return (
        <div style={bgStyle}>
            {props.children}
        </div>
    );
    
    
}

