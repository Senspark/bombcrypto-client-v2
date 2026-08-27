import { useEffect, useRef, useState } from "react";
import { UnityInstance } from "react-unity-webgl/declarations/unity-instance";
import {
    computeDpr,
    DeviceTier,
    probeCpuWeakness,
    resolveTier,
    storeTier,
} from "../utils/DevicePerformance.ts";

const SAMPLE_WINDOW_MS = 5000;
const LOW_FPS_THRESHOLD = 40;
const LOW_FPS_WINDOWS_TO_DOWNGRADE = 2;
const WATCHDOG_WARMUP_MS = 15000;
const RESIZE_DEBOUNCE_MS = 250;

const LIMIT_EXPLANATION = {
    screen: "vừa đúng số pixel màn hình cấp cho canvas",
    budget: "chạm ngân sách pixel của tier",
    "tier-ceiling": "chạm trần dpr của tier",
    floor: "chạm sàn dpr",
} as const;

type UnityModuleWithDpr = UnityInstance["Module"] & {
    devicePixelRatio?: number;
    canvas?: HTMLCanvasElement;
};

/**
 * Chọn devicePixelRatio theo sức máy, theo kích thước canvas thật sự hiện trên màn hình,
 * và theo FPS đo được.
 *
 * - Giá trị trả về dùng cho prop `devicePixelRatio` của <Unity>; react-unity-webgl chốt prop
 *   này lúc mount nên nó chỉ là giá trị khởi tạo.
 * - Sau khi Unity chạy, hook ghi thẳng `Module.devicePixelRatio`. Unity đọc lại giá trị đó mỗi
 *   lần poll screen size (`_JS_SystemInfo_GetPreferredDevicePixelRatio`) nên đổi lúc runtime có
 *   hiệu lực ngay, không cần reload.
 * - Canvas chỉ bị `transform: scale` trong fullratio mode, nên `ResizeObserver` không đủ:
 *   phải nghe cả `resize` của window và state `zoom`.
 */
export function useAdaptiveDevicePixelRatio(
    instance: UnityInstance | null,
    fallbackWidth: number,
    fallbackHeight: number,
    zoom: number,
) {
    const [resolved] = useState(resolveTier);
    const tierRef = useRef<DeviceTier>(resolved.tier);
    const tierReasonRef = useRef<string>(
        `${resolved.source}: ${resolved.reasons.join(", ") || "không có tín hiệu máy yếu"}`,
    );
    const [dpr, setDpr] = useState(
        () =>
            resolved.forcedDpr ??
            computeDpr(
                {
                    cssWidth: fallbackWidth,
                    cssHeight: fallbackHeight,
                    renderedWidth: fallbackWidth,
                    renderedHeight: fallbackHeight,
                },
                window.devicePixelRatio || 1,
                resolved.tier,
            ).dpr,
    );

    const applyRef = useRef<() => void>(() => {});

    useEffect(() => {
        console.log(`[dpr] tier=${resolved.tier} (${tierReasonRef.current})`);
        if (resolved.forcedDpr !== null) {
            console.log(`[dpr] ${resolved.forcedDpr} — ép cứng qua ?dpr=, bỏ qua mọi tự động`);
        }
    }, [resolved]);

    useEffect(() => {
        if (!instance) {
            return;
        }
        const unityModule = instance.Module as UnityModuleWithDpr;

        const apply = () => {
            if (resolved.forcedDpr !== null) {
                unityModule.devicePixelRatio = resolved.forcedDpr;
                return;
            }
            const canvas = unityModule.canvas;
            // getBoundingClientRect() đã tính CSS transform, clientWidth thì không.
            const rect = canvas?.getBoundingClientRect();
            const cssWidth = canvas?.clientWidth || fallbackWidth;
            const cssHeight = canvas?.clientHeight || fallbackHeight;
            const screenDpr = window.devicePixelRatio || 1;
            const tier = tierRef.current;
            const decision = computeDpr(
                {
                    cssWidth,
                    cssHeight,
                    renderedWidth: rect?.width || cssWidth,
                    renderedHeight: rect?.height || cssHeight,
                },
                screenDpr,
                tier,
            );
            if (unityModule.devicePixelRatio === decision.dpr) {
                return;
            }
            unityModule.devicePixelRatio = decision.dpr;
            setDpr(decision.dpr);

            const buffer = `${Math.round(cssWidth * decision.dpr)}x${Math.round(cssHeight * decision.dpr)}`;
            console.log(
                `[dpr] ${decision.dpr} — ${LIMIT_EXPLANATION[decision.limitedBy]}\n` +
                    `      canvas css ${cssWidth}x${cssHeight}, hiện trên màn ${Math.round(decision.visibleWidth)}x${Math.round(decision.visibleHeight)}, screenDpr ${screenDpr}\n` +
                    `      screen cần ${decision.needed.toFixed(2)} | budget cho ${decision.affordable.toFixed(2)} | trần tier ${decision.ceiling}\n` +
                    `      tier ${tier} (${tierReasonRef.current}) -> render buffer ${buffer}`,
            );
        };

        applyRef.current = apply;
        apply();

        let resizeTimer: ReturnType<typeof setTimeout> | undefined;
        const applyDebounced = () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(apply, RESIZE_DEBOUNCE_MS);
        };

        const observer = unityModule.canvas ? new ResizeObserver(applyDebounced) : null;
        if (unityModule.canvas && observer) {
            observer.observe(unityModule.canvas);
        }
        window.addEventListener("resize", applyDebounced);
        window.addEventListener("fullscreenchange", apply);

        // Kéo cửa sổ sang màn hình khác hoặc ctrl+zoom -> devicePixelRatio đổi mà `resize`
        // không phải lúc nào cũng bắn. Query chỉ khớp đúng giá trị hiện tại nên phải nạp lại
        // sau mỗi lần đổi.
        let dprQuery: MediaQueryList | null = null;
        const onDprChange = () => {
            applyDebounced();
            watchDpr();
        };
        const watchDpr = () => {
            dprQuery?.removeEventListener("change", onDprChange);
            dprQuery = window.matchMedia(`(resolution: ${window.devicePixelRatio}dppx)`);
            dprQuery.addEventListener("change", onDprChange);
        };
        watchDpr();

        return () => {
            clearTimeout(resizeTimer);
            observer?.disconnect();
            window.removeEventListener("resize", applyDebounced);
            window.removeEventListener("fullscreenchange", apply);
            dprQuery?.removeEventListener("change", onDprChange);
        };
    }, [instance, fallbackWidth, fallbackHeight, resolved]);

    // zoom đổi -> transform đổi -> kích thước hiện trên màn hình đổi, mà không sự kiện DOM nào bắn.
    useEffect(() => {
        applyRef.current();
    }, [zoom]);

    const downgrade = (reason: string) => {
        if (tierRef.current === "low") {
            return;
        }
        tierRef.current = "low";
        tierReasonRef.current = reason;
        console.log(`[dpr] hạ tier xuống low — ${reason}`);
        applyRef.current();
    };

    // UA Client Hints là async nên chỉ về sau khi đã render vài frame ở tier tạm.
    useEffect(() => {
        if (resolved.forcedDpr !== null || tierRef.current === "low") {
            return;
        }
        let cancelled = false;
        probeCpuWeakness().then((reason) => {
            if (!cancelled && reason) {
                downgrade(`ua-client-hints: ${reason}`);
            }
        });
        return () => {
            cancelled = true;
        };
    }, [resolved]);

    useEffect(() => {
        if (!instance || resolved.forcedDpr !== null || tierRef.current === "low") {
            return;
        }

        let frames = 0;
        let windowStart = performance.now();
        let slowWindows = 0;
        let rafId = 0;
        let started = false;

        const tick = (now: number) => {
            // Bỏ qua giai đoạn đầu: decompress asset và khởi tạo scene luôn tụt FPS.
            if (!started) {
                if (now - windowStart < WATCHDOG_WARMUP_MS) {
                    rafId = requestAnimationFrame(tick);
                    return;
                }
                started = true;
                windowStart = now;
            }

            frames += 1;
            const elapsed = now - windowStart;
            if (elapsed >= SAMPLE_WINDOW_MS) {
                // Tab chạy nền bị throttle rAF xuống ~1fps — bỏ cửa sổ đó đi, không phải máy yếu.
                // Đây đúng là kiểu dùng của user mở nhiều Chrome profile cùng lúc.
                if (document.hidden || elapsed > SAMPLE_WINDOW_MS * 2) {
                    frames = 0;
                    windowStart = now;
                    rafId = requestAnimationFrame(tick);
                    return;
                }
                const fps = (frames * 1000) / elapsed;
                frames = 0;
                windowStart = now;
                slowWindows = fps < LOW_FPS_THRESHOLD ? slowWindows + 1 : 0;
                if (slowWindows >= LOW_FPS_WINDOWS_TO_DOWNGRADE) {
                    storeTier("low");
                    downgrade(
                        `fps ${fps.toFixed(1)} < ${LOW_FPS_THRESHOLD} trong ${LOW_FPS_WINDOWS_TO_DOWNGRADE} cửa sổ liên tiếp (đã nhớ cho lần sau)`,
                    );
                    return;
                }
            }
            rafId = requestAnimationFrame(tick);
        };

        rafId = requestAnimationFrame(tick);
        return () => cancelAnimationFrame(rafId);
    }, [instance, resolved]);

    return dpr;
}
