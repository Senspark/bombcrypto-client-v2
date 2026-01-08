using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IMatchHeroInfo {
        int Id { get; }
        int Color { get; }
        int Skin { get; }

        [NotNull]
        Dictionary<int, int[]> SkinChests { get; }

        int Health { get; }
        int Speed { get; }
        int Damage { get; }
        int BombCount { get; }
        int BombRange { get; }
        int MaxHealth { get; }
        int MaxSpeed { get; }
        int MaxDamage { get; }
        int MaxBombCount { get; }
        int MaxBombRange { get; }
    }
}