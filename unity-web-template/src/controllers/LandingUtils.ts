// Cross-mode switch (Treasure ⇄ Adventure).
// Mọi môi trường (local/staging/prod) cùng 1 deployment, mode chọn bằng query ?landing=.
// → URL mode khác = giữ nguyên origin + pathname của tab hiện tại, thay query bằng ?landing=<mode>.
export type LandingMode = 'treasure' | 'adventure';

// Mode hiện tại của tab, suy từ ?landing=. Default 'treasure' (khớp Unity RuntimeConfig).
export function getCurrentLanding(): LandingMode {
    const landing = new URL(window.location.href).searchParams.get('landing');
    return landing === 'adventure' ? 'adventure' : 'treasure';
}

export function buildLandingUrl(mode: LandingMode): string {
    return `${window.location.origin}${window.location.pathname}?landing=${mode}`;
}

// Mở mode trong tab mới (2 mode chạy song song — đúng mục tiêu multi-tab coexistence).
export function openLandingMode(mode: LandingMode): void {
    window.open(buildLandingUrl(mode), '_blank');
}
