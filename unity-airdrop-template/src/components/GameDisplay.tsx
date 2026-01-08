import React, { useContext, useEffect, useRef, useState, useCallback } from "react";
import { Unity, useUnityContext } from "react-unity-webgl";
import gameBorder from "@assets/game_border.png";
import fullscreen_button from "@assets/fullscreen_button.png";
import fullratio_button from "@assets/fullratio_button.png";
import { EnvConfig } from "../configs/EnvConfig.ts";
import {unityService, walletService} from "../hooks/GlobalServices.ts";
import { UnityInstance } from "react-unity-webgl/declarations/unity-instance";
import { StyleContext } from "./StyleContext.tsx";
import { debounce } from "../utils/Debounce.ts";
import YouTubePlayer from './YouTubePlayer';

declare global {
    interface Window {
        unityInstance: UnityInstance | null;
        enableBgVideo: (val: boolean) => void;
    }
}
const maxWidth = 1000;
const maxHeight = 620;

export default function GameDisplay() {
    const [zoom, setZoom] = useState(1);
    const styleContext = useContext(StyleContext);
    const canAutoResize = useRef(false);
    const [showVideo, enableBgVideo] = useState(false);

    if (!styleContext) {
        throw new Error("StyleContext must be used within a StyleProvider");
    }

    const { fullratio, setfullratio } = styleContext;
    const unityFolder = EnvConfig.unityFolder();
    const loaderUrlExtension = EnvConfig.loaderExtension();
    const dataUrlExtension = EnvConfig.dataExtension();
    const frameworkUrlExtension = EnvConfig.frameworkExtension();
    const codeUrlExtension = EnvConfig.codeExtension();

    const loaderUrl = `${unityFolder}${loaderUrlExtension}`;
    const dataUrl = `${unityFolder}${dataUrlExtension}`;
    const frameworkUrl = `${unityFolder}${frameworkUrlExtension}`;
    const codeUrl = `${unityFolder}${codeUrlExtension}`;

    const { unityProvider, UNSAFE__unityInstance } = useUnityContext({
        loaderUrl,
        dataUrl,
        frameworkUrl,
        codeUrl,
        streamingAssetsUrl: `${unityFolder}/StreamingAssets`,
    });

    useEffect(() => {
        if (UNSAFE__unityInstance) {
            window.unityInstance = UNSAFE__unityInstance;
            unityService.setUnityInstance(UNSAFE__unityInstance);
        }
    }, [UNSAFE__unityInstance]);

    const handleResize = useCallback(() => {
        if (canAutoResize.current) {
            const containerWidth = maxWidth;
            const containerHeight = maxHeight;
            const windowWidth = window.outerWidth;
            const windowHeight = window.innerHeight;

            const zoomWidth = windowWidth / containerWidth;
            const zoomHeight = windowHeight / containerHeight;
            const zoomFactor = Math.min(zoomWidth, zoomHeight);
            setZoom(zoomFactor);
        }
    }, [maxWidth, maxHeight]);

    const debouncedHandleResize = debounce(handleResize, 100);

    useEffect(() => {
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") {
                resetZoom();
            }
        };

        window.addEventListener("keydown", handleKeyDown);
        return () => {
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, [debouncedHandleResize]);

    const resetZoom = () => {
        setZoom(1);
        setfullratio(false);
        canAutoResize.current = false;
        window.removeEventListener("resize", debouncedHandleResize);
    };

    useEffect(() => {
        window.enableBgVideo = enableBgVideo;
        if(walletService.getConnection() !== true){
            enableBgVideo(true);
        }
        
    }, []);

    const handlefullratioClick = () => {
        const containerWidth = maxWidth;
        const containerHeight = maxHeight;
        const windowWidth = window.outerWidth;
        const windowHeight = window.innerHeight;

        const zoomWidth = windowWidth / containerWidth;
        const zoomHeight = windowHeight / containerHeight;
        const zoomFactor = Math.min(zoomWidth, zoomHeight);
        setZoom(zoomFactor);
        setfullratio(true);
        canAutoResize.current = true;
        window.addEventListener("resize", debouncedHandleResize);
    };

    return (
        <div style={fullratio ? containerStylefullratio(zoom) : containerStyleDefault(zoom)}>
            <div style={{
                    ...(fullratio ? iframeContainerStylefullratio : iframeContainerStyleDefault),
                    position: "relative",
                }}
            >
                <Unity
                    devicePixelRatio={2}
                    matchWebGLToCanvasSize={true}
                    unityProvider={unityProvider}
                    style={{ width: "100%", height: "100%" }}
                />
                {showVideo && (
                    <div style={wrapperVideo(fullratio)}>
                        <YouTubePlayer style={videoStyle} />
                    </div>
                )}
                <div style={{ ...wrapperImg, display: fullratio ? "none" : "flex" }} />
                <div
                    style={{ ...fullratioButton, display: fullratio ? "none" : "flex" }}
                    onClick={handlefullratioClick}
                />
                <div
                    style={{ ...FullScreenButton, display: fullratio ? "none" : "flex" }}
                    onClick={() => window.unityInstance?.SetFullscreen(1)}
                />
            </div>
        </div>
    );
}

const containerStyleDefault = (zoom: number): React.CSSProperties =>({
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    width: "100%",
    height: undefined,
    position: "relative",
    transform: `scale(${zoom})`,
});

const containerStylefullratio = (zoom: number): React.CSSProperties => ({
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    height: "100%",
    position: "fixed",
    top: "50%",
    left: "50%",
    transform: `translate(-50%, -50%) scale(${zoom})`,
});

const iframeContainerStyleDefault: React.CSSProperties = {
    position: "relative",
    width: maxWidth,
    height: maxHeight,
    justifyContent: "center",
    alignItems: "center",
    padding: "15px",
};

const iframeContainerStylefullratio: React.CSSProperties = {
    position: "relative",
    width: maxWidth,
    height: maxHeight,
    justifyContent: "center",
    alignItems: "center",
};

const wrapperImg: React.CSSProperties = {
    backgroundImage: `url(${gameBorder})`,
    backgroundRepeat: "no-repeat",
    backgroundSize: "100% 100%",
    backgroundPosition: "top",
    pointerEvents: "none",
    cursor: "pointer",
    position: "absolute",
    top: 0,
    left: 0,
    width: "100%",
    height: "100%",
    maxWidth: "1000px",
    maxHeight: "629px",
    zIndex: 2,
    display: "flex",
};

const wrapperVideo = (fullratio: boolean): React.CSSProperties => ({
    pointerEvents: "none",
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    width: fullratio ? "101%" : "96%",
    height: fullratio ? "100%" : "96%",
    zIndex: 1,
    objectFit: "cover",
    backgroundColor: "black",
});

const videoStyle: React.CSSProperties = {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    width: "100%",
    height: "100%",
    objectFit: "cover",
};

const FullScreenButton: React.CSSProperties = {
    width: "38px",
    height: "38px",
    backgroundImage: `url(${fullscreen_button})`,
    cursor: "pointer",
    position: "absolute",
    top: "100%",
    right: "93%",
    backgroundRepeat: "no-repeat",
    overflow: "visible",
    display: "flex",
    pointerEvents: "auto",
};

const fullratioButton: React.CSSProperties = {
    width: "38px",
    height: "38px",
    backgroundImage: `url(${fullratio_button})`,
    cursor: "pointer",
    position: "absolute",
    top: "100%",
    right: "88%",
    backgroundRepeat: "no-repeat",
    overflow: "visible",
    display: "flex",
    pointerEvents: "auto",
    backgroundSize: "contain",
};