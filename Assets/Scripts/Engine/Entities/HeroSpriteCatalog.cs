using System.Collections.Generic;
using App;

namespace Engine.Entities {
    /// <summary>
    /// Bảng LUẬT (data = law) ánh xạ <see cref="PlayerType"/> → cách dựng path sprite hero trên đĩa.
    /// Thay cho serialized dict trong <c>AnimationResource(.asset)</c>: loader tự ghép path theo
    /// convention thay vì nhồi từng <see cref="UnityEngine.Sprite"/> theo giá trị vào bundle.
    ///
    /// Source of truth: filesystem
    ///   <c>Assets/Data/CharactersSprites/{Folder}/{colorSeg}/{action}/{prefix}_{NN}.png</c>
    /// (đã verify md5 dedup TON ⊃ WEB — xem <c>docs/more_skin/report_phase0_findings.md</c>).
    ///
    /// <para><b>Folder lưu tường minh</b> (không dùng <c>Enum.ToString()</c>) để Phase 5 tách
    /// <see cref="PlayerType"/> → BHeroSkin/HeroTRSkin không biến thành breaking change về path.</para>
    ///
    /// <para>Tổng quát hoá <see cref="BHeroSkinPalette"/>: thêm HeroTr range (Poo..Hesman) + cờ
    /// <c>HasDie</c>/<c>HasSkin</c>/<c>HasIcon</c>. Phần tử [0] của bộ màu = màu fallback ưu tiên
    /// (= <see cref="Entry.DefaultColor"/>): White cho BHero, HeroTr cho hero chỉ-TR.</para>
    /// </summary>
    public static class HeroSpriteCatalog {
        public const string BasePath = "Assets/Data/CharactersSprites";

        public sealed class Entry {
            /// <summary>Tên folder trên đĩa (decoupled khỏi enum name).</summary>
            public readonly string Folder;

            /// <summary>Bộ màu có art; [0] = màu fallback ưu tiên.</summary>
            public readonly IReadOnlyList<PlayerColor> Colors;

            /// <summary>Màu fallback khi requested không có art (= Colors[0]).</summary>
            public readonly PlayerColor DefaultColor;

            /// <summary>Có folder <c>Die/die_NN</c> (4 frame) không.</summary>
            public readonly bool HasDie;

            /// <summary>Có nhánh <c>Skin/{rarity}/...</c> không.</summary>
            public readonly bool HasSkin;

            /// <summary>Có <c>icon.png</c> riêng (dùng cho portrait, vd DogeTr) không.</summary>
            public readonly bool HasIcon;

            public Entry(string folder, IReadOnlyList<PlayerColor> colors,
                bool hasDie = false, bool hasSkin = false, bool hasIcon = false) {
                Folder = folder;
                Colors = colors;
                DefaultColor = colors.Count > 0 ? colors[0] : PlayerColor.White;
                HasDie = hasDie;
                HasSkin = hasSkin;
                HasIcon = hasIcon;
            }

            public bool Supports(PlayerColor color) {
                foreach (var c in Colors) {
                    if (c == color) return true;
                }
                return false;
            }
        }

        // Bộ màu tái dùng (phần tử [0] = fallback ưu tiên).
        private static readonly PlayerColor[] Full =
            { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow };
        private static readonly PlayerColor[] FullTr =
            { PlayerColor.White, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Red, PlayerColor.Yellow, PlayerColor.HeroTr };
        private static readonly PlayerColor[] WhiteOnly = { PlayerColor.White };
        private static readonly PlayerColor[] WhiteTr = { PlayerColor.White, PlayerColor.HeroTr };
        private static readonly PlayerColor[] TrOnly = { PlayerColor.HeroTr };
        private static readonly PlayerColor[] BlueRed = { PlayerColor.Blue, PlayerColor.Red };
        private static readonly PlayerColor[] WhiteRed = { PlayerColor.White, PlayerColor.Red };
        private static readonly PlayerColor[] RedOnly = { PlayerColor.Red };

        // Bảng dựng từ thực tế đĩa (Assets/Data/CharactersSprites). 54 hero = enum PlayerType.
        // (Block=0 là placeholder, không có folder → không liệt kê.)
        private static readonly Dictionary<PlayerType, Entry> Table = new() {
            // --- BHero (FI) — kênh store/airdrop, ordinals 1..31 ---
            [PlayerType.BomberMan]       = new Entry("BomberMan", Full, hasSkin: true),
            [PlayerType.Knight]          = new Entry("Knight", FullTr, hasDie: true, hasSkin: true),
            [PlayerType.Man]             = new Entry("Man", FullTr, hasDie: true, hasSkin: true),
            [PlayerType.Vampire]         = new Entry("Vampire", Full, hasSkin: true),
            [PlayerType.Witch]           = new Entry("Witch", FullTr, hasDie: true, hasSkin: true),
            [PlayerType.Doge]            = new Entry("Doge", WhiteOnly, hasSkin: true),
            [PlayerType.Pepe]            = new Entry("Pepe", WhiteOnly, hasSkin: true),
            [PlayerType.Ninja]           = new Entry("Ninja", WhiteTr, hasDie: true, hasSkin: true),
            [PlayerType.King]            = new Entry("King", WhiteOnly, hasSkin: true),
            [PlayerType.PilotRabit]      = new Entry("PilotRabit", WhiteOnly),
            [PlayerType.Meo2]            = new Entry("Meo2", WhiteOnly),
            [PlayerType.Monkey]          = new Entry("Monkey", WhiteOnly),
            [PlayerType.Pilot]           = new Entry("Pilot", WhiteOnly),
            [PlayerType.BlackCat]        = new Entry("BlackCat", WhiteOnly),
            [PlayerType.Tiger]           = new Entry("Tiger", WhiteOnly, hasDie: true),
            [PlayerType.PugDog]          = new Entry("PugDog", WhiteOnly),
            [PlayerType.SailorMoon]      = new Entry("SailorMoon", WhiteOnly),
            [PlayerType.PepeClown]       = new Entry("PepeClown", WhiteOnly),
            [PlayerType.FrogGentlemen]   = new Entry("FrogGentlemen", WhiteOnly),
            [PlayerType.Dragoon]         = new Entry("Dragoon", WhiteOnly, hasDie: true),
            [PlayerType.Ghost]           = new Entry("Ghost", WhiteOnly),
            [PlayerType.Pumpkin]         = new Entry("Pumpkin", WhiteOnly),
            [PlayerType.Werewolves]      = new Entry("Werewolves", WhiteOnly),
            [PlayerType.FootballFrog]    = new Entry("FootballFrog", WhiteOnly),
            [PlayerType.FootballKnight]  = new Entry("FootballKnight", WhiteOnly),
            [PlayerType.FootballMan]     = new Entry("FootballMan", WhiteOnly),
            [PlayerType.FootballVampire] = new Entry("FootballVampire", WhiteOnly),
            [PlayerType.FootballWitch]   = new Entry("FootballWitch", WhiteOnly),
            [PlayerType.FootballDoge]    = new Entry("FootballDoge", WhiteOnly),
            [PlayerType.FootballPepe]    = new Entry("FootballPepe", WhiteOnly),
            [PlayerType.FootballNinja]   = new Entry("FootballNinja", WhiteOnly),

            // --- HeroTR — kênh traditional, ordinals 111..133 (hack +100, client-only) ---
            [PlayerType.Poo]             = new Entry("Poo", TrOnly, hasDie: true),
            [PlayerType.GKu]             = new Entry("GKu", TrOnly, hasDie: true),
            [PlayerType.PinkyToon]       = new Entry("PinkyToon", TrOnly, hasDie: true),
            [PlayerType.Stickman]        = new Entry("Stickman", TrOnly, hasDie: true),
            [PlayerType.Monitor]         = new Entry("Monitor", TrOnly, hasDie: true),
            [PlayerType.Dragon]          = new Entry("Dragon", TrOnly, hasDie: true),
            [PlayerType.Santa]           = new Entry("Santa", TrOnly, hasDie: true),
            [PlayerType.Miner]           = new Entry("Miner", TrOnly, hasDie: true),
            [PlayerType.Calico]          = new Entry("Calico", TrOnly, hasDie: true),
            [PlayerType.Kuroneko]        = new Entry("Kuroneko", TrOnly, hasDie: true),
            [PlayerType.GoldenKat]       = new Entry("GoldenKat", TrOnly, hasDie: true),
            [PlayerType.MrDear]          = new Entry("MrDear", TrOnly, hasDie: true),
            [PlayerType.TLion]           = new Entry("TLion", TrOnly, hasDie: true),
            [PlayerType.Frog]            = new Entry("Frog", TrOnly, hasDie: true, hasSkin: true),
            [PlayerType.DogeTr]          = new Entry("DogeTr", TrOnly, hasDie: true, hasIcon: true),
            [PlayerType.KingTr]          = new Entry("KingTr", RedOnly, hasDie: true),
            [PlayerType.Cupid]           = new Entry("Cupid", TrOnly, hasDie: true),
            [PlayerType.BGuy]            = new Entry("BGuy", TrOnly, hasDie: true),
            [PlayerType.PinkyBear]       = new Entry("PinkyBear", TrOnly, hasDie: true),
            [PlayerType.PinkyNeko]       = new Entry("PinkyNeko", RedOnly, hasDie: true),
            [PlayerType.Dragoon2]        = new Entry("Dragoon2", TrOnly, hasDie: true),
            [PlayerType.FatTiger]        = new Entry("FatTiger", TrOnly, hasDie: true),
            [PlayerType.Hesman]          = new Entry("Hesman", TrOnly, hasDie: true),

            // --- Super skins (BHero, ordinals 32..58) — mượn folder HeroTR (new_plan.md cột 3).
            //     Chỉ màu HeroTr, có Die, không Skin. SuperDogeTr có icon.png cho portrait. ---
            [PlayerType.SuperKnight]     = new Entry("Knight", TrOnly, hasDie: true),
            [PlayerType.SuperCowboy]     = new Entry("Man", TrOnly, hasDie: true),
            [PlayerType.SuperWitch]      = new Entry("Witch", TrOnly, hasDie: true),
            [PlayerType.SuperNinja]      = new Entry("Ninja", TrOnly, hasDie: true),
            [PlayerType.SuperPoo]        = new Entry("Poo", TrOnly, hasDie: true),
            [PlayerType.SuperGKu]        = new Entry("GKu", TrOnly, hasDie: true),
            [PlayerType.SuperPinkyToon]  = new Entry("PinkyToon", TrOnly, hasDie: true),
            [PlayerType.SuperStickman]   = new Entry("Stickman", TrOnly, hasDie: true),
            [PlayerType.SuperMonitor]    = new Entry("Monitor", TrOnly, hasDie: true),
            [PlayerType.SuperDragon]     = new Entry("Dragon", TrOnly, hasDie: true),
            [PlayerType.SuperSanta]      = new Entry("Santa", TrOnly, hasDie: true),
            [PlayerType.SuperMiner]      = new Entry("Miner", TrOnly, hasDie: true),
            [PlayerType.SuperCalico]     = new Entry("Calico", TrOnly, hasDie: true),
            [PlayerType.SuperKuroneko]   = new Entry("Kuroneko", TrOnly, hasDie: true),
            [PlayerType.SuperGoldenKat]  = new Entry("GoldenKat", TrOnly, hasDie: true),
            [PlayerType.SuperMrDear]     = new Entry("MrDear", TrOnly, hasDie: true),
            [PlayerType.SuperTLion]      = new Entry("TLion", TrOnly, hasDie: true),
            [PlayerType.SuperFrog]       = new Entry("Frog", TrOnly, hasDie: true),
            [PlayerType.SuperDogeTr]     = new Entry("DogeTr", TrOnly, hasDie: true, hasIcon: true),
            [PlayerType.SuperKingTr]     = new Entry("KingTr", BlueRed, hasDie: true),
            [PlayerType.SuperCupid]      = new Entry("Cupid", TrOnly, hasDie: true),
            [PlayerType.SuperBGuy]       = new Entry("BGuy", TrOnly, hasDie: true),
            [PlayerType.SuperPinkyBear]  = new Entry("PinkyBear", TrOnly, hasDie: true),
            [PlayerType.SuperNekoChan]   = new Entry("PinkyNeko", WhiteRed, hasDie: true),
            [PlayerType.SuperHesman]     = new Entry("Hesman", TrOnly, hasDie: true),

            // Chess skins (BHero, 57..61) — Red+Blue, default Blue.
            [PlayerType.ChessQueen]      = new Entry("ChessQueen", BlueRed, hasDie: true),
            [PlayerType.ChessPawn]       = new Entry("ChessPawn", BlueRed, hasDie: true),
            [PlayerType.ChessRook]       = new Entry("ChessRook", BlueRed, hasDie: true),
            [PlayerType.ChessHorse]      = new Entry("ChessHorse", BlueRed, hasDie: true),
            [PlayerType.ChessBishop]     = new Entry("ChessBishop", BlueRed, hasDie: true),

            // BHero skins mới (đơn màu White), 62..66.
            [PlayerType.Irondeux]        = new Entry("Irondeux", WhiteOnly, hasDie: true),
            [PlayerType.Omega]           = new Entry("Omega", WhiteOnly, hasDie: true),
            [PlayerType.Saber]           = new Entry("Saber", WhiteOnly, hasDie: true),
            [PlayerType.DemonSlayer]     = new Entry("DemonSlayer", WhiteOnly, hasDie: true),
            [PlayerType.KoiMan]          = new Entry("KoiMan", WhiteOnly, hasDie: true),
        };

        public static bool TryGet(PlayerType type, out Entry entry) => Table.TryGetValue(type, out entry);

        public static Entry Get(PlayerType type) => Table.TryGetValue(type, out var e) ? e : null;

        public static bool Has(PlayerType type) => Table.ContainsKey(type);

        /// <summary>
        /// Resolve màu yêu cầu → màu thật có art:
        ///   không có entry → trả requested; support requested → requested; còn lại → DefaultColor.
        /// (Đồng dạng <see cref="BHeroSkinPalette.Resolve"/> nhưng phủ cả HeroTr range.)
        /// </summary>
        public static PlayerColor ResolveColor(PlayerType type, PlayerColor requested) {
            if (!Table.TryGetValue(type, out var e)) return requested;
            return e.Supports(requested) ? requested : e.DefaultColor;
        }
    }
}
