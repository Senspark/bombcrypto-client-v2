using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

using PvpMode.Manager;

namespace BLPvpMode.Manager.Builders {
    public class MatchUserInfoBuilder : IMatchUserInfoBuilder {
        public bool IsBot { get; set; }

        [CanBeNull]
        public string DisplayName { get; set; }

        public int Rank { get; set; }

        [CanBeNull]
        public BoosterType[] Boosters { get; set; }

        public int Skin { get; set; }

        [CanBeNull]
        public Dictionary<int, int[]> SkinChests { get; set; }

        public int Health { get; set; }
        public int Speed { get; set; }
        public int Damage { get; set; }
        public int BombCount { get; set; }
        public int BombRange { get; set; }

        public IMatchUserInfo Build() {
            return new MatchUserInfo {
                IsBot = IsBot,
                Username = "",
                DisplayName = DisplayName ?? "null",
                Rank = Rank,
                Boosters = (Boosters ?? Array.Empty<BoosterType>())
                    .Select(DefaultBoosterManager.ConvertFromEnum)
                    .ToArray(),
                Hero = new MatchHeroInfo {
                    Skin = Skin,
                    SkinChests = SkinChests ?? new Dictionary<int, int[]>(),
                    Health = Health,
                    Speed = Speed,
                    Damage = Damage,
                    BombCount = BombCount,
                    BombRange = BombRange,
                    MaxSpeed = 10,
                    MaxBombCount = 10,
                    MaxBombRange = 10,
                },
            };
        }
    }
}