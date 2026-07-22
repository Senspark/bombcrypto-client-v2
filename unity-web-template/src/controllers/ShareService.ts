import Logger from "./Logger.ts";
import ApiService from "./ApiService.ts";

// Payload do Unity gửi qua tag SHARE_ON_X (xem Assets/Share/Scripts/Social/ShareHeroDto.cs).
// heroes[i] = [heroId, rarity, skin] (positional). Unity gửi id thô; React tự map tên (Quyết định A)
// -> đổi câu chữ / tên rarity / tên skin chỉ sửa file này, KHÔNG build lại WebGL.
type HeroTuple = [number, number, number]; // [heroId, rarity, skin]

interface SharePayload {
    image: string;            // base64 JPG (không kèm tiền tố "data:")
    source: string;           // "new_hero" | "summary" | "token_reward" | "collection"
    heroes?: HeroTuple[] | null;
    token?: string;           // token_reward: tên token hiển thị
    amount?: number;          // token_reward: số lượng claim
}

// rarity:int (PlayerData.rare 0..9) -> tên. Đối chiếu HeroRarityDisplay.Rarity bên Unity.
const RARITY_NAMES = [
    "Common", "Rare", "Super Rare", "Epic", "Legend",
    "Super Legend", "Mega", "Super Mega", "Mystic", "Super Mystic",
];

// skin:int = (int)PlayerData.playerType (ordinal enum PlayerType, BHero pool 1..66) -> tên hiển thị.
// Đối chiếu Assets/Scripts/Engine/Entities/Enum/PlayerType.cs. Tên ở đây thuần hiển thị, sửa tự do.
const SKIN_NAMES: Record<number, string> = {
    1: "BomberMan", 2: "Knight", 3: "Man", 4: "Vampire", 5: "Witch",
    6: "Doge", 7: "Pepe", 8: "Ninja", 9: "King", 10: "Pilot Rabbit",
    11: "Meo", 12: "Monkey", 13: "Pilot", 14: "Black Cat", 15: "Tiger",
    16: "Pug Dog", 17: "Sailor Moon", 18: "Pepe Clown", 19: "Frog Gentleman", 20: "Dragoon",
    21: "Ghost", 22: "Pumpkin", 23: "Werewolf", 24: "Football Frog", 25: "Football Knight",
    26: "Football Man", 27: "Football Vampire", 28: "Football Witch", 29: "Football Doge", 30: "Football Pepe",
    31: "Football Ninja",
    32: "Super Knight", 33: "Super Cowboy", 34: "Super Witch", 35: "Super Ninja", 36: "Super Poo",
    37: "Super GKu", 38: "Super Pinky Toon", 39: "Super Stickman", 40: "Super Monitor", 41: "Super Dragon",
    42: "Super Santa", 43: "Super Miner", 44: "Super Calico", 45: "Super Kuroneko", 46: "Super Golden Kat",
    47: "Super Mr Dear", 48: "Super T-Lion", 49: "Super Frog", 50: "Super Doge", 51: "Super King",
    52: "Super Cupid", 53: "Super B-Guy", 54: "Super Pinky Bear", 55: "Super Neko Chan", 56: "Super Hesman",
    57: "Chess Queen", 58: "Chess Pawn", 59: "Chess Rook", 60: "Chess Horse", 61: "Chess Bishop",
    62: "Irondeux", 63: "Omega", 64: "Saber", 65: "Demon Slayer", 66: "Koi Man",
};

const X_INTENT_URL = "https://x.com/intent/tweet";
// Tag thiết yếu nhúng thẳng vào body để tiết kiệm chữ (vd "on #BombCrypto" thay vì "on Bombcrypto … #BombCrypto",
// "1,234.56 #BCOIN" thay vì "1,234.56 BCOIN") -> luôn được giữ.
const PLATFORM_TAG = "#BombCrypto";
// Hashtag phụ, đặt cuối -> bỏ trước nếu tweet vượt budget.
const SECONDARY_HASHTAGS = "#Play2Earn #TreasureHunt";
// X t.co rút mọi URL về 23 ký tự; chừa 23 + 1 space để text + url <= 280.
const TEXT_BUDGET = 280 - 23 - 1;

const TAG = "[Share]";

export default class ShareService {
    constructor(
        private readonly _logger: Logger,
        private readonly _apiService: ApiService,
    ) {
    }

    // Unity gọi qua window.jsCall('SHARE_ON_X', cb, json). Trả "OK" / "ERROR:<msg>".
    public async shareOnX(dataJson: string): Promise<string> {
        // Mở tab trống NGAY (giữ user-activation, chống popup-block sau await).
        const tab = window.open("", "_blank");
        try {
            const payload = JSON.parse(dataJson) as SharePayload;
            const backend = await this.createShare(payload);

            // Backend soạn text + sinh trang OG (kèm ảnh). Lỗi/chưa cấu hình -> text-only local.
            const text = backend?.text ?? this.composeText(payload);
            const url = backend?.url ?? "";

            if (!tab) {
                return "ERROR:popup_blocked";
            }

            const params = url
                ? `text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`
                : `text=${encodeURIComponent(text)}`;
            tab.location.href = `${X_INTENT_URL}?${params}`;
            return "OK";
        } catch (e) {
            if (tab) {
                tab.close();
            }
            const msg = (e as Error)?.message ?? String(e);
            this._logger.error(`${TAG} shareOnX failed: ${msg}`);
            return `ERROR:${msg}`;
        }
    }

    private composeText(payload: SharePayload): string {
        let body: string;
        switch (payload.source) {
            case "token_reward":
                body = this.composeTokenBody(payload);
                break;
            case "summary":
                body = this.composeSummaryBody(payload.heroes ?? []);
                break;
            case "collection":
                body = `Check out my BHero collection on ${PLATFORM_TAG}`;
                break;
            case "new_hero":
            default:
                body = this.composeNewHeroBody(payload.heroes ?? []);
                break;
        }
        return this.withHashtags(body);
    }

    // Tag thiết yếu (#BombCrypto, #<token>) đã nằm trong body -> luôn giữ. Chỉ thêm hashtag phụ nếu còn chỗ;
    // body cực dài (hiếm) mới phải cắt.
    private withHashtags(body: string): string {
        const full = `${body} ${SECONDARY_HASHTAGS}`;
        if (full.length <= TEXT_BUDGET) {
            return full;
        }
        if (body.length <= TEXT_BUDGET) {
            return body;
        }
        return `${body.slice(0, TEXT_BUDGET - 1)}…`;
    }

    private composeTokenBody(payload: SharePayload): string {
        const amount = this.formatAmount(payload.amount ?? 0);
        // Luôn dùng cashtag $BCOIN bất kể token thật là gì (quyết định sản phẩm).
        return `I just claimed ${amount} $BCOIN on ${PLATFORM_TAG}`;
    }

    private composeNewHeroBody(heroes: HeroTuple[]): string {
        if (heroes.length === 0) {
            return `Just got a new BHero on ${PLATFORM_TAG}`;
        }
        const [, rarity, skin] = heroes[0];
        return `I just minted a ${this.rarityName(rarity)} ${this.skinName(skin)} on ${PLATFORM_TAG}`;
    }

    private composeSummaryBody(heroes: HeroTuple[]): string {
        const count = heroes.length;
        if (count === 0) {
            return `Just minted new BHeroes on ${PLATFORM_TAG}`;
        }
        const topRarity = Math.max(...heroes.map(([, rarity]) => rarity));
        const heroWord = count === 1 ? "BHero" : "BHeroes";
        return `I just minted ${count} ${heroWord} ${this.rarityName(topRarity)} on ${PLATFORM_TAG}`;
    }

    private rarityName(rarity: number): string {
        return RARITY_NAMES[rarity] ?? `Rarity ${rarity}`;
    }

    private skinName(skin: number): string {
        return SKIN_NAMES[skin] ?? `BHero #${skin}`;
    }

    private formatAmount(amount: number): string {
        return amount.toLocaleString("en-US", {maximumFractionDigits: 4});
    }

    // Gửi ảnh + data sang backend, trả { url, text }. Không ảnh hoặc lỗi -> null (fallback text-only).
    private async createShare(payload: SharePayload): Promise<{ url: string; text: string } | null> {
        if (!payload.image) {
            return null;
        }
        try {
            return await this._apiService.createShare(payload);
        } catch (e) {
            this._logger.error(`${TAG} createShare failed, fallback text-only: ${(e as Error).message}`);
            return null;
        }
    }
}
