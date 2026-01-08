using System;
using System.Collections.Generic;
using System.Numerics;

using CodeStage.AntiCheat.ObscuredTypes;

using Sfs2X.Entities.Data;

using App;

using Scenes.TreasureModeScene.Scripts.Solana.Server_Response;

namespace Server.Models {
    public class HeroDetails : IHeroDetails {
        public string Details => _details;
        public int Id { get; protected set; }
        public int Rarity { get; }
        public HeroAccountType Type { get; }
        public int Level { get; }
        public int LevelShield { get; }
        public int NumResetShield { get; }
        public int Color { get; }
        public int Skin { get; }
        public int Stamina { get; }
        public int Speed { get; set; }
        public int BombSkin { get; }
        public int BombCount { get; set; }
        public int BombPower { get; }
        public int BombRange { get; set; }
        public int[] Abilities { get; }
        public int RandomizeAbilityCounter { get; }
        public bool IsHeroS { get; }

        public List<IHeroSAbility> HeroSAbilities { get; }
        public IHeroSAbility Shield { get; set; }
        public bool IsActive { get; set; }
        public int Energy { get; set; }
        public HeroStage Stage { get; set; }
        public int StoryHp { get; }
        public bool StoryIsPlayed { get; set; }
        public long TimeSync { get; set; }
        public HeroAccountType AccountType { get; }

        public bool AllowRevive { get; }
        public bool AllowReviveByAds { get; }
        public int ReviveGemAmount { get; }
        public long TimeLockSince { get; }
        public int TimeLockSeconds { get; }
        public double StakeBcoin { get; set; }
        public double StakeSen { get; set; }

        private readonly ObscuredString _details;

        public static IHeroDetails[] ParseArray(ISFSObject data) {
            var detailsArray = data.GetSFSArray("bombers") ?? data.GetSFSArray("data");
            var details = new IHeroDetails[detailsArray.Size()];
            for (var i = 0; i < detailsArray.Size(); ++i) {
                var entry = detailsArray.GetSFSObject(i);
                details[i] = Parse(entry);
            }
            return details;
        }

        public static IHeroDetails Parse(ISFSObject data) {
            var details = data.GetUtfString("gen_id");
            var accountType = HeroAccountType.Trial;
            if (data.ContainsKey(SFSDefine.SFSField.AccountType)) {
                accountType = (HeroAccountType)data.GetInt(SFSDefine.SFSField.AccountType);
            } else if (data.ContainsKey(SFSDefine.SFSField.HeroType)) {
                accountType = (HeroAccountType)data.GetInt(SFSDefine.SFSField.HeroType);
            }
            var id = (int)data.GetLong("id");
            var active = data.GetInt("active") == 1;
            var stage = (HeroStage)data.GetInt("stage");
            var energy = data.GetInt("energy");
            var restoredEnergy = data.GetInt("restore_hp");
            var storyHp = data.GetInt("story_hp");
            var storyIsPlayed = data.GetBool("story_is_played");
            var stakeBcoin = data.GetDouble("stake_bcoin");
            var stakeSen = data.GetDouble("stake_sen");
            var heroSAbility = HeroSAbility.ParseArrayAbilities(data);

            var extraData = data.GetSFSObject("data") ?? data;
            if (accountType == HeroAccountType.Nft) {
                return new HeroDetails(details, id, active, stage, energy + restoredEnergy, storyHp, storyIsPlayed,
                    heroSAbility,extraData, stakeBcoin, stakeSen, accountType);
            }
            return new HeroDetails(extraData, id, active, stage, energy + restoredEnergy, accountType, heroSAbility, accountType);
        }

        /// <summary>
        /// Using Server Data (Trial Hero)
        /// </summary>
        private HeroDetails(ISFSObject data, int id, bool active, HeroStage stage, int energy,
            HeroAccountType accountType, List<IHeroSAbility> heroSAbilities, HeroAccountType type) {
            Id = id;
            IsActive = active;
            Stage = stage;
            Energy = energy;
            TimeSync = DateTime.Now.ToBinary();
            Type = type;
            StoryHp = 0;
            StoryIsPlayed = true;
            HeroSAbilities = heroSAbilities;
            AccountType = accountType;
            StakeBcoin = 0;
            StakeSen = 0;

            Rarity = data.GetInt("rare");
            Level = data.GetInt("level");
            Color = data.GetInt("playercolor");
            Skin = data.GetInt("playerType");
            Stamina = data.GetInt("stamina");
            Speed = data.GetInt("speed");
            BombSkin = data.GetInt("bombSkin");
            BombCount = data.GetInt("bombNum");
            BombPower = data.GetInt("bombDamage");
            BombRange = data.GetInt("bombRange");
            Abilities = data.GetIntArray("abilities");
            RandomizeAbilityCounter = 0;
            IsHeroS = true;
            LevelShield = 0;
            NumResetShield = 0;
            TimeLockSince = 0;
            Shield = HeroSAbilities.Find(e => e.AbilityType == HeroSAbilityType.AvoidThunder);
        }

        /// <summary>
        /// Using Gen Id (NFT)
        /// </summary>
        private HeroDetails(string details, int id, bool active, HeroStage stage, int energy, int storyHp,
            bool storyIsPlayed, List<IHeroSAbility> heroSAbilities, ISFSObject data, double stakeBcoin, double stakeSen,
            HeroAccountType type) : this(details) {
            if (string.IsNullOrWhiteSpace(details)) {
                Id = id;
            }

            IsActive = active;
            Stage = stage;
            Energy = energy;
            TimeSync = DateTime.Now.ToBinary();    
            Type = type;
            StoryHp = storyHp;
            StoryIsPlayed = storyIsPlayed;
            HeroSAbilities = heroSAbilities;
            if (data.ContainsKey("lock_since")) {
                TimeLockSince = data.GetLong("lock_since");
            }
            TimeLockSeconds = data.GetInt("lock_seconds");
            AccountType = HeroAccountType.Nft;
            StakeBcoin = stakeBcoin;
            StakeSen = stakeSen;
            Shield = HeroSAbilities.Find(e => e.AbilityType == HeroSAbilityType.AvoidThunder);
        }

        /// <summary>
        /// Using Gen Id
        /// </summary>
        public HeroDetails(string details) {
            if (string.IsNullOrWhiteSpace(details)) {
                return;
            }
            _details = details;
            var detailsInt = BigInteger.Parse(details);
            /* Giải thích về bitshift
             * BigInterger có độ dài là 255 bit
             * 
             * Trường "Id" được chứa ở 30 bit đầu tiên, nên sẽ dùng phép & ((1 << 30) - 1) để lấy ra
             * Giải thích như sau:
             * 1 (dec)              = ...00000001 (binary)
             * 1 << 30              = 1000000000000000000000000000000 (binary) (Thêm 30 số 0 vào phía sau -> 31 bit)
             * (1 << 30) - 1        = 111111111111111111111111111111 (binary) (còn 30 bit giá trị 1)
             * detailsInt & (...)   = lấy 30 bit đầu tiên của detailsInt
             *
             * Trường index được chứa ở 10 bit tiếp theo, tính từ 30 bit đầu tiên
             * Giải thích như sau:
             * detailsInt >> 30     = cắt bỏ 30 bit đầu tiên của detailsInt
             * (detailsInt >> 30) & ((1 << 10) - 1) = lấy 10 bit đầu tiên của detailsInt (sau khi đã cắt bỏ 30 bit)
             */
            var n30Bits = (BigInteger)(1 << 30) - 1; // Tương đương với Math.Pow(2, 30) - 1
            var n10Bits = (BigInteger)(1 << 10) - 1;
            var n5Bits = (BigInteger)(1 << 5) - 1;
            Id = (int)(detailsInt & n30Bits);
            var index = (int)((detailsInt >> 30) & n10Bits); // unused 
            Rarity = (int)((detailsInt >> 40) & n5Bits);
            Level = (int)((detailsInt >> 45) & n5Bits);
            Color = (int)((detailsInt >> 50) & n5Bits);
            Skin = (int)((detailsInt >> 55) & n5Bits);
            Stamina = (int)((detailsInt >> 60) & n5Bits);
            Speed = (int)((detailsInt >> 65) & n5Bits);
            BombSkin = (int)((detailsInt >> 70) & n5Bits);
            BombCount = (int)((detailsInt >> 75) & n5Bits);
            BombPower = (int)((detailsInt >> 80) & n5Bits);
            BombRange = (int)((detailsInt >> 85) & n5Bits);
            var ability = (int)((detailsInt >> 90) & n5Bits);
            Abilities = new int[ability];
            for (var i = 0; i < ability; ++i) {
                Abilities[i] = (int)((detailsInt >> (95 + i * 5)) & n5Bits);
            }
            var blockNumber = (int)((detailsInt >> 145) & n30Bits);
            RandomizeAbilityCounter = (int)((detailsInt >> 175) & n5Bits);
            var heroSAbilitiesLength = (int)((detailsInt >> 180) & n5Bits);
            IsHeroS = heroSAbilitiesLength > 0;
            var heroSAbilities2 = new int[heroSAbilitiesLength];
            for (var i = 0; i < heroSAbilitiesLength; i++) {
                heroSAbilities2[i] = (int)((detailsInt >> 185 + i * 5) & n5Bits);
            }
            LevelShield = (int)((detailsInt >> 235) & n5Bits);
            NumResetShield = (int)((detailsInt >> 240) & n10Bits);
            HeroSAbilities = new List<IHeroSAbility>();
            Shield = HeroSAbilities.Find(e => e.AbilityType == HeroSAbilityType.AvoidThunder);
        }

        private class HeroSAbility : IHeroSAbility {
            public int CurrentAmount { get; }
            public int TotalAmount { get; }
            public HeroSAbilityType AbilityType { get; }

            private HeroSAbility(ISFSObject data) {
                CurrentAmount = data.GetInt("current");
                TotalAmount = data.GetInt("total");
                AbilityType = (HeroSAbilityType)(data.GetInt("ability") - 1);
            }

            public static List<IHeroSAbility> ParseArrayAbilities(ISFSObject data) {
                var heroSAbility = new List<IHeroSAbility>();
                var shield = data.GetSFSArray("shields");
                if (shield != null) {
                    foreach (ISFSObject s in shield) {
                        heroSAbility.Add(new HeroSAbility(s));
                    }
                }
                return heroSAbility;
            }
        }
    }
}