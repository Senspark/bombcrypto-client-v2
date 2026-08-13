export const MAX_DPR = 2;
export const MIN_DPR = 0.5;

const STORAGE_KEY = "bomb:dpr-tier";

// Ngân sách pixel cho một frame. Baseline hiện tại (canvas 1000x620 @ dpr 2) là ~2.5M pixel,
// nên tier "high" giữ nguyên hành vi cũ ở chế độ thường và chỉ cắt khi vào fullscreen.
const PIXEL_BUDGET: Record<DeviceTier, number> = {
    high: 2_500_000,
    low: 1_000_000,
};

// Trần cứng theo tier. Cần tách khỏi PIXEL_BUDGET vì ngân sách tính theo diện tích: canvas
// nhỏ (mobile, ~380x300 CSS px) vẫn lọt qua budget ở dpr 2 dù máy yếu. Mobile luôn rơi vào
// tier low (hard signal) nên trần này chính là "mobile tối đa dpr 1".
const DPR_CEILING: Record<DeviceTier, number> = {
    high: MAX_DPR,
    low: 1,
};

// GPU render bằng phần mềm -> ép tier low ngay, không cần tín hiệu nào khác.
const SOFTWARE_RENDERER = /swiftshader|llvmpipe|software|basic render|microsoft basic/i;
// iGPU / mobile GPU đời cũ: tín hiệu mềm, cần thêm một tín hiệu nữa mới hạ.
const WEAK_GPU =
    /(Intel).*(HD Graphics (2|3|4|5)\d{2}|UHD Graphics 6[0-2]0|Iris.*5\d{3})|Mali-[TG]?[2-6]\d{2}|Adreno \(TM\) [3-5]\d{2}/i;

const FOUR_K_PIXELS = 3840 * 2160;

// Smart TV / set-top box: CPU ARM yếu, màn 4K, gần như luôn nghẹn ở dpr 2.
const TV_DEVICE =
    /SMART-TV|SmartTV|Tizen|Web0S|webOS|NetCast|BRAVIA|HbbTV|AFT[A-Z]|CrKey|GoogleTV|Android TV|VIDAA|Roku|PhilipsTV/i;
// GPU của SBC nhúng: Raspberry Pi báo "VideoCore" hoặc "V3D".
const EMBEDDED_GPU = /VideoCore|V3D|Broadcom/i;
// navigator.platform trên Linux/ARM: "Linux aarch64", "Linux armv7l", "Linux armv8l".
// Apple Silicon khai "MacIntel" nên không dính nhánh này.
const ARM_PLATFORM = /arm|aarch64/i;
const X86_ARCH = /x86|amd64/i;
// Mac chạy GPU rời/tích hợp của Intel-AMD-NVIDIA = máy Intel đời cũ (2020 trở về trước).
// KHÔNG dùng navigator.platform để phân biệt: Apple Silicon cũng khai "MacIntel".
const APPLE_SILICON_GPU = /Apple M\d/i;
const INTEL_MAC_GPU = /Intel|AMD|Radeon|NVIDIA|GeForce/i;

export type DeviceTier = "high" | "low";

function readGpuRenderer(): string {
    try {
        const canvas = document.createElement("canvas");
        const gl = (canvas.getContext("webgl2") ??
            canvas.getContext("webgl")) as WebGLRenderingContext | null;
        if (!gl) {
            return "";
        }
        const info = gl.getExtension("WEBGL_debug_renderer_info");
        const renderer = info
            ? gl.getParameter(info.UNMASKED_RENDERER_WEBGL)
            : gl.getParameter(gl.RENDERER);
        gl.getExtension("WEBGL_lose_context")?.loseContext();
        return typeof renderer === "string" ? renderer : "";
    } catch {
        return "";
    }
}

export function screenPhysicalPixels(): number {
    const ratio = window.devicePixelRatio || 1;
    return window.screen.width * ratio * window.screen.height * ratio;
}

export type DeviceProbe = {
    tier: DeviceTier;
    reasons: string[];
    renderer: string;
};

export function probeDevice(): DeviceProbe {
    const renderer = readGpuRenderer();
    const cores = navigator.hardwareConcurrency ?? 8;
    const memory = (navigator as Navigator & { deviceMemory?: number }).deviceMemory;
    // iPadOS Safari khai UA là "Macintosh"; maxTouchPoints là cách duy nhất phân biệt.
    const isIpadOs = /Macintosh/.test(navigator.userAgent) && navigator.maxTouchPoints > 1;
    const isMobile =
        (navigator as Navigator & { userAgentData?: { mobile?: boolean } }).userAgentData?.mobile ||
        /Android|iPhone|iPad|iPod/i.test(navigator.userAgent) ||
        isIpadOs;
    const pixels = screenPhysicalPixels();

    const hard: string[] = [];
    const soft: string[] = [];

    const platform = navigator.platform ?? "";
    const isMac = /Mac/i.test(platform) && !isIpadOs;

    if (SOFTWARE_RENDERER.test(renderer)) hard.push(`software-renderer:${renderer}`);
    if (isMobile) hard.push("mobile");
    if (cores <= 2) hard.push(`cores:${cores}`);
    if (TV_DEVICE.test(navigator.userAgent)) hard.push("smart-tv");
    if (EMBEDDED_GPU.test(renderer)) hard.push(`embedded-gpu:${renderer}`);
    if (ARM_PLATFORM.test(platform) && !isMac) hard.push(`arm-platform:${platform}`);
    if (isMac && !APPLE_SILICON_GPU.test(renderer) && INTEL_MAC_GPU.test(renderer)) {
        hard.push(`intel-mac:${renderer}`);
    }

    if (cores <= 4) soft.push(`cores:${cores}`);
    if (memory !== undefined && memory <= 4) soft.push(`memory:${memory}`);
    if (WEAK_GPU.test(renderer)) soft.push(`weak-gpu:${renderer}`);
    // Màn hình vốn dpr 1: render ở 2 chỉ là supersampling, tốn 4x diện tích pixel.
    if ((window.devicePixelRatio || 1) <= 1) soft.push("screen-dpr:1");
    // Màn 4K+ trên máy ít nhân: laptop 4K panel + iGPU, gần như luôn nghẹn ở fullscreen.
    if (pixels >= FOUR_K_PIXELS && cores <= 8) soft.push(`screen-pixels:${Math.round(pixels / 1e6)}M`);

    const weak = hard.length > 0 || soft.length >= 2;
    return {
        tier: weak ? "low" : "high",
        reasons: weak ? [...hard, ...soft] : [],
        renderer,
    };
}

type UserAgentData = {
    getHighEntropyValues?: (hints: string[]) => Promise<{
        architecture?: string;
        platform?: string;
    }>;
};

/**
 * Kiến trúc CPU qua User-Agent Client Hints — nguồn sạch nhất nhưng chỉ có trên Chromium và
 * bắt buộc async, nên chạy sau `probeDevice()` để tinh chỉnh chứ không chặn lúc mount.
 *
 * Bù được hai chỗ mù của nhánh sync:
 * - Windows-on-ARM (Surface Pro X): UA khai "Windows NT", `navigator.platform` là "Win32".
 * - Mac Intel trên Safari: Safari che GPU renderer thành "Apple GPU" cho cả Intel lẫn
 *   Apple Silicon nên không phân biệt được bằng renderer.
 *
 * Trên macOS phải đọc `architecture` chứ tuyệt đối không đọc `platform`: Apple Silicon khai
 * `navigator.platform === "MacIntel"` y hệt máy Intel thật.
 */
export async function probeCpuWeakness(): Promise<string | null> {
    const uaData = (navigator as Navigator & { userAgentData?: UserAgentData }).userAgentData;
    if (!uaData?.getHighEntropyValues) {
        return null;
    }
    try {
        const hints = await uaData.getHighEntropyValues(["architecture", "platform"]);
        const architecture = hints.architecture ?? "";
        if (hints.platform === "macOS") {
            return X86_ARCH.test(architecture) ? "intel-mac" : null;
        }
        return ARM_PLATFORM.test(architecture)
            ? `arm-cpu:${hints.platform || "unknown"}`
            : null;
    } catch {
        return null;
    }
}

export type CanvasMetrics = {
    /** Kích thước layout của canvas — Unity nhân dpr vào đây để ra drawing buffer. */
    cssWidth: number;
    cssHeight: number;
    /** Kích thước canvas thật sự chiếm trên màn hình, đã tính CSS transform (`scale`). */
    renderedWidth: number;
    renderedHeight: number;
};

/**
 * dpr = min(đủ nét trên màn hình, vừa ngân sách pixel của tier).
 *
 * Vế thứ nhất là cái bắt được trường hợp user thu nhỏ cửa sổ / mở nhiều Chrome profile:
 * canvas chỉ hiện ra 400px thì render 2000px là phí thẳng 25 lần diện tích. Vế thứ hai
 * chặn trường hợp ngược lại (fullscreen 4K) khi màn hình đòi nhiều hơn máy kham nổi.
 *
 * Bám bậc DPR_STEP để resize không tạo ra chuỗi đổi render target liên tục, nhưng hai vế
 * làm tròn ngược hướng nhau: `needed` lên (render thiếu là mờ thấy ngay — browser zoom 90%
 * cho screenDpr 0.9, làm tròn xuống sẽ rơi thẳng xuống 0.5), `affordable` xuống (là trần).
 */
const DPR_STEP = 0.25;

/** Ràng buộc nào đã quyết định giá trị cuối — để log ra được lý do chứ không chỉ con số. */
export type DprLimit = "screen" | "budget" | "tier-ceiling" | "floor";

export type DprDecision = {
    dpr: number;
    limitedBy: DprLimit;
    needed: number;
    affordable: number;
    ceiling: number;
    visibleWidth: number;
    visibleHeight: number;
};

export function computeDpr(
    metrics: CanvasMetrics,
    screenDpr: number,
    tier: DeviceTier,
): DprDecision {
    const cssWidth = Math.max(1, metrics.cssWidth);
    const cssHeight = Math.max(1, metrics.cssHeight);

    const visibleWidth = Math.min(metrics.renderedWidth, window.innerWidth);
    const visibleHeight = Math.min(metrics.renderedHeight, window.innerHeight);
    const needed = Math.max(
        (visibleWidth * screenDpr) / cssWidth,
        (visibleHeight * screenDpr) / cssHeight,
    );
    const neededStepped = Math.ceil(needed / DPR_STEP) * DPR_STEP;

    const affordable = Math.sqrt(PIXEL_BUDGET[tier] / (cssWidth * cssHeight));
    const affordableStepped = Math.floor(affordable / DPR_STEP) * DPR_STEP;

    const ceiling = DPR_CEILING[tier];
    const capped = Math.min(neededStepped, affordableStepped, ceiling);
    const dpr = Math.max(MIN_DPR, capped);

    let limitedBy: DprLimit = "screen";
    if (capped < MIN_DPR) limitedBy = "floor";
    else if (ceiling <= affordableStepped && ceiling <= neededStepped) limitedBy = "tier-ceiling";
    else if (affordableStepped < neededStepped) limitedBy = "budget";

    return { dpr, limitedBy, needed, affordable, ceiling, visibleWidth, visibleHeight };
}

function forcedDevicePixelRatio(): number | null {
    const raw = new URLSearchParams(window.location.search).get("dpr");
    if (!raw) {
        return null;
    }
    const value = Number(raw);
    return Number.isFinite(value) && value > 0 ? value : null;
}

function readStoredTier(): DeviceTier | null {
    try {
        const value = localStorage.getItem(STORAGE_KEY);
        return value === "low" || value === "high" ? value : null;
    } catch {
        return null;
    }
}

export function storeTier(tier: DeviceTier): void {
    try {
        localStorage.setItem(STORAGE_KEY, tier);
    } catch {
        /* private mode */
    }
}

function clearStoredTier(): void {
    try {
        localStorage.removeItem(STORAGE_KEY);
    } catch {
        /* private mode */
    }
}

export type ResolvedTier = {
    tier: DeviceTier;
    forcedDpr: number | null;
    source: "url" | "stored" | "probe";
    reasons: string[];
};

export function resolveTier(): ResolvedTier {
    const forcedDpr = forcedDevicePixelRatio();
    if (forcedDpr !== null) {
        clearStoredTier();
        return { tier: "high", forcedDpr, source: "url", reasons: [] };
    }

    const stored = readStoredTier();
    if (stored !== null) {
        return { tier: stored, forcedDpr: null, source: "stored", reasons: ["previous-session"] };
    }

    const probe = probeDevice();
    return { tier: probe.tier, forcedDpr: null, source: "probe", reasons: probe.reasons };
}
