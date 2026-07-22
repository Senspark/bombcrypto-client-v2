using System.Collections.Generic;

namespace Engine.Entities {
    /// <summary>
    /// Tường minh map BHero (Hero FI) skin → các PlayerColor có art asset.
    /// Source of truth: filesystem
    ///   Assets/Scenes/TreasureModeScene/Textures/Pack/Characters/{playerType}/{color}/
    ///   Assets/Scenes/MainMenuScene/Textures/Pack/Characters/{playerType}/{color}/
    /// và file skin.md ở repo root (đồng bộ thủ công khi thêm/sửa color asset).
    ///
    /// Phạm vi: chỉ cover BHero (Hero FI). HeroTr (Poo, GKu, KingTr, ..., Hesman) không có entry;
    /// Resolve sẽ trả về requested không đổi và caller (Avatar.cs / AnimationResource) tự xử lý
    /// theo logic riêng của TR (thường fallback chain qua White → HeroTr).
    ///
    /// Convention: phần tử [0] của list là preferred fallback color (thường là White).
    /// Edge cases:
    ///   - Doge / Pepe có { White, Skin } — không có Blue/Green/Red/Yellow/HeroTr.
    ///   - Hesman: KHÔNG nằm trong palette mặc dù DefaultPlayerStoreManager.GetPlayerType(31)
    ///     map skin index 31 sang PlayerType.Hesman. Hesman thuộc TR enum range (= 133), asset
    ///     chỉ có HeroTr folder, fallback qua Avatar.cs chain (White → HeroTr).
    ///
    /// Note về PlayerColor.Skin: chỉ TreasureModeScene có folder `Characters/{type}/Skin/{rarity}/...`,
    /// MainMenuScene không có. Caller phải biết context (airdrop vs non-airdrop) khi muốn dùng Skin path.
    /// IsSkinSupported() chỉ trả lời "playerType có Skin folder không?" — không phân biệt scene.
    /// </summary>
    public static class BHeroSkinPalette {
        private static readonly Dictionary<PlayerType, IReadOnlyList<PlayerColor>> SupportedColors = new() {
            // BHero đủ dải màu cơ bản (Blue/Green/Red/White/Yellow) + Skin
            [PlayerType.BomberMan]       = new[] { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.Skin },
            [PlayerType.Knight]          = new[] { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.HeroTr, PlayerColor.Skin },
            [PlayerType.Man]             = new[] { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.HeroTr, PlayerColor.Skin },
            [PlayerType.Vampire]         = new[] { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.Skin },
            [PlayerType.Witch]           = new[] { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.HeroTr, PlayerColor.Skin },

            // BHero giới hạn nhưng có Skin
            [PlayerType.Doge]            = new[] { PlayerColor.White, PlayerColor.Skin },
            [PlayerType.Pepe]            = new[] { PlayerColor.White, PlayerColor.Skin },
            [PlayerType.Ninja]           = new[] { PlayerColor.White, PlayerColor.HeroTr, PlayerColor.Skin },
            [PlayerType.King]            = new[] { PlayerColor.White, PlayerColor.Skin },

            // BHero chỉ có White (không có Skin folder)
            [PlayerType.PilotRabit]      = new[] { PlayerColor.White },
            [PlayerType.Meo2]            = new[] { PlayerColor.White },
            [PlayerType.Monkey]          = new[] { PlayerColor.White },
            [PlayerType.Pilot]           = new[] { PlayerColor.White },
            [PlayerType.BlackCat]        = new[] { PlayerColor.White },
            [PlayerType.Tiger]           = new[] { PlayerColor.White },
            [PlayerType.PugDog]          = new[] { PlayerColor.White },
            [PlayerType.SailorMoon]      = new[] { PlayerColor.White },
            [PlayerType.PepeClown]       = new[] { PlayerColor.White },
            [PlayerType.FrogGentlemen]   = new[] { PlayerColor.White },
            [PlayerType.Dragoon]         = new[] { PlayerColor.White },
            [PlayerType.Ghost]           = new[] { PlayerColor.White },
            [PlayerType.Pumpkin]         = new[] { PlayerColor.White },
            [PlayerType.Werewolves]      = new[] { PlayerColor.White },
            [PlayerType.FootballFrog]    = new[] { PlayerColor.White },
            [PlayerType.FootballKnight]  = new[] { PlayerColor.White },
            [PlayerType.FootballMan]     = new[] { PlayerColor.White },
            [PlayerType.FootballVampire] = new[] { PlayerColor.White },
            [PlayerType.FootballWitch]   = new[] { PlayerColor.White },
            [PlayerType.FootballDoge]    = new[] { PlayerColor.White },
            [PlayerType.FootballPepe]    = new[] { PlayerColor.White },
            [PlayerType.FootballNinja]   = new[] { PlayerColor.White },
            // Hesman intentionally NOT listed: nằm trong TR range, không phải BHero.
            // Asset chỉ có HeroTr; Avatar.cs fallback chain (White → HeroTr) sẽ catch.
        };

        /// <summary>
        /// Trả về true nếu playerType nằm trong palette (là BHero / Hero FI skin).
        /// </summary>
        public static bool IsBHeroSkin(PlayerType playerType) => SupportedColors.ContainsKey(playerType);

        /// <summary>
        /// Trả về true nếu BHero này có folder PlayerColor.Skin (rarity-keyed art).
        /// Lưu ý: folder Skin chỉ tồn tại ở TreasureModeScene; MainMenuScene không có.
        /// </summary>
        public static bool IsSkinSupported(PlayerType playerType) {
            if (!SupportedColors.TryGetValue(playerType, out var colors)) return false;
            foreach (var c in colors) {
                if (c == PlayerColor.Skin) return true;
            }
            return false;
        }

        /// <summary>
        /// Resolve PlayerColor để render BHero skin:
        ///   - Nếu playerType không có entry trong palette (HeroTr / non-BHero) → trả về requested không đổi.
        ///   - Nếu requested nằm trong supported list → trả về requested.
        ///   - Nếu requested không support → trả về fallback color (phần tử [0]) — thường là White,
        ///     ngoại lệ Hesman fallback về HeroTr.
        /// </summary>
        public static PlayerColor Resolve(PlayerType playerType, PlayerColor requested) {
            if (!SupportedColors.TryGetValue(playerType, out var colors)) {
                return requested;
            }
            foreach (var c in colors) {
                if (c == requested) return requested;
            }
            return colors[0];
        }
    }
}
