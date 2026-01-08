import { Unity } from "react-unity-webgl";
import "./UnityRenderer.css";
import { EnvConfig } from "../services/EnvConfig.ts";
import { useContext, useEffect } from "react";
import { UnityInstance } from "react-unity-webgl/declarations/unity-instance";
import { StyleContext } from "./StyleContext.tsx";
import { useUnitySetup } from "../hooks/useUnitySetup";
import { useResize } from "../hooks/useResize";
import { 
  maxWidth, 
  maxHeight,
  getContainerStyleDefault,
  getContainerStyleFullRatio,
  iframeContainerStyleDefault,
  iframeContainerStyleFullRatio,
  wrapperImg,
  fullRatioButton,
  fullScreenButton
} from "./UnityRenderer.styles";

/**
 * Unity WebGL Renderer component
 * Handles rendering and interaction with the Unity WebGL instance
 */
export const UnityRenderer = () => {
    // Get style context for full ratio state
    const styleContext = useContext(StyleContext);
    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }
    
    const { fullRatio, setFullRatio } = styleContext;
    const unityFolder = EnvConfig.unityFolder();
    
    // Set up Unity context
    const { unityProvider } = useUnitySetup({
        unityFolder
    });
    
    // Handler for resetting zoom
    const handleResetZoom = () => {
        setFullRatio(false);
    };
    
    // Set up resize functionality
    const { zoom, isFullRatioMode, handleFullRatioClick } = useResize({
        maxWidth,
        maxHeight,
        onResetZoom: handleResetZoom
    });
    
    // Sync fullRatio state with isFullRatioMode from the resize hook
    useEffect(() => {
        setFullRatio(isFullRatioMode);
    }, [isFullRatioMode, setFullRatio]);
    
    // Update fullRatio state when zoom changes
    const onFullRatioClick = () => {
        handleFullRatioClick();
    };
    
    // Handle fullscreen button click
    const handleFullscreenClick = () => {
        window.unityInstance?.SetFullscreen(1);
    };

    return (
        <div style={fullRatio ? getContainerStyleFullRatio(zoom) : getContainerStyleDefault(zoom)}
             className={fullRatio ? "full-ratio-mode" : ""}>
            <div style={fullRatio ? iframeContainerStyleFullRatio : iframeContainerStyleDefault}>
                <Unity 
                    devicePixelRatio={2}
                    matchWebGLToCanvasSize={true}
                    unityProvider={unityProvider}
                    style={{width: '100%', height: '100%'}}
                />
                <div style={{...wrapperImg, display: fullRatio ? "none" : "flex"}}/>
                <div
                    style={{...fullRatioButton, display: fullRatio ? "none" : "flex"}}
                    onClick={onFullRatioClick}
                />
                <div
                    style={{...fullScreenButton, display: fullRatio ? "none" : "flex"}}
                    onClick={handleFullscreenClick}
                />
            </div>
        </div>
    );
};

/**
 * TypeScript declaration for global window object
 * to access Unity instance from the window object
 */
declare global {
    interface Window {
        unityInstance: UnityInstance;
    }
}