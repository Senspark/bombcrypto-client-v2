import {Col, Row} from "antd";
import React, {useContext} from "react";
import {StyleContext} from "./StyleContext.tsx";
import { Footer } from "antd/es/layout/layout";

export default function AppFooter() {
    const styleContext = useContext(StyleContext);

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const { fullRatio } = styleContext;
    if (fullRatio) {
        return null;
    }

    return (
        <Footer style={footerStyle}>
            <Row align="middle">
                <Col span={12}>
                    <appkit-button/>
                </Col>
            </Row>
        </Footer>
    );
}
const footerStyle: React.CSSProperties = {
    maxHeight: 80,
};