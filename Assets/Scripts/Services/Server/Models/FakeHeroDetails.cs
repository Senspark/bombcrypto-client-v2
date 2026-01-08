using System.Collections.Generic;
using System.Numerics;

using App;

using UnityEngine;
using UnityEngine.Assertions;

namespace Server.Models {
    public class FakeHeroDetails : IHeroDetails {
        public string Details { get; set; }
        public int Id { get; set; }
        public int Rarity { get; set; }
        public HeroAccountType Type { get; }
        public int Level { get; set; }
        public int LevelShield { get; set; }
        public int NumResetShield { get; set; }
        public int Color { get; set; }
        public int Skin { get; set; }
        public int Stamina { get; set; }
        public int Speed { get; set; }
        public int BombSkin { get; set; }
        public int BombCount { get; set; }
        public int BombPower { get; set; }
        public int BombRange { get; set; }
        public int[] Abilities { get; set; }
        public int RandomizeAbilityCounter { get; set; }
        public bool IsActive { get; set; }
        public int Energy { get; set; }
        public HeroStage Stage { get; set; }
        public bool IsHeroS { get; set; }
        public List<IHeroSAbility> HeroSAbilities { get; set; }
        public IHeroSAbility Shield { get; set; }
        public int StoryHp { get; set; }
        public bool StoryIsPlayed { get; set; }
        public long TimeSync { get; set; }
        public HeroAccountType AccountType { get; set; }
        public bool AllowRevive { get; set; }
        public bool AllowReviveByAds { get; set; }
        public int ReviveGemAmount { get; set; }
        public long TimeLockSince { get; }
        public int TimeLockSeconds { get; }
        public double StakeBcoin { get; set; }
        public double StakeSen { get; set; }

        public static void Test() {
            var hero = new FakeHeroDetails() {
                NumResetShield = 0,
                LevelShield = 0,
                HeroSAbilities = new List<IHeroSAbility>(),
                RandomizeAbilityCounter = 0,
                Abilities = new[] { 1, 2, 3, 4, 5, 6, 7 },
                BombRange = 5,
                BombPower = 5,
                BombCount = 3,
                BombSkin = 1,
                Speed = 5,
                Stamina = 5,
                Skin = 1,
                Color = 1,
                Level = 1,
                Rarity = 1,
                Id = 1,
            };
            Debug.Log(EncodeToGenId(hero));
            var org = new HeroDetails(EncodeToGenId(hero));
            Debug.Log(org.Details);
        }

        public static string EncodeToGenId(IHeroDetails details) {
            BigInteger detailsInt = 0;
            var index = (BigInteger)(1 << 10) - 1;
            var blockNumber = (BigInteger)(1 << 10) - 1;

            detailsInt = (BigInteger)details.Id;
            detailsInt |= index << 30;
            detailsInt |= (BigInteger)details.Rarity << 40;
            detailsInt |= (BigInteger)details.Level << 45;
            detailsInt |= (BigInteger)details.Color << 50;
            detailsInt |= (BigInteger)details.Skin << 55;
            detailsInt |= (BigInteger)details.Stamina << 60;
            detailsInt |= (BigInteger)details.Speed << 65;
            detailsInt |= (BigInteger)details.BombSkin << 70;
            detailsInt |= (BigInteger)details.BombCount << 75;
            detailsInt |= (BigInteger)details.BombPower << 80;
            detailsInt |= (BigInteger)details.BombRange << 85;
            Assert.IsTrue(details.Abilities.Length <= 10);
            detailsInt |= (BigInteger)details.Abilities.Length << 90;
            for (var i = 0; i < details.Abilities.Length; i++) {
                detailsInt |= (BigInteger)details.Abilities[i] << (95 + i * 5);
            }
            detailsInt |= (BigInteger)blockNumber << 145;
            detailsInt |= (BigInteger)details.RandomizeAbilityCounter << 175;
            Assert.IsTrue(details.HeroSAbilities.Count <= 10);
            detailsInt |= (BigInteger)details.HeroSAbilities.Count << 180;
            for (var i = 0; i < details.HeroSAbilities.Count; i++) {
                detailsInt |= (BigInteger)(int)details.HeroSAbilities[i].AbilityType << (185 + i * 5);
            }
            detailsInt |= (BigInteger)details.LevelShield << 235;
            detailsInt |= (BigInteger)details.NumResetShield << 240;

            return detailsInt.ToString();
        }
    }
}