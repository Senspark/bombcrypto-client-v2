using System.Collections.Generic;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public enum HeroDamageSource {
        Null,
        Bomb,
        HardBlock,
        PrisonBreak,
    }

    public enum HeroItem {
        // Boosters.
        BombUp,
        FireUp,
        Boots,

        // Rewards.
        Gold,
        BronzeChest,
        SilverChest,
        GoldChest,
        PlatinumChest,
    }

    public enum HeroEffect {
        Shield,
        Invincible,
        Imprisoned,

        // Skull effects.
        SpeedTo1,
        SpeedTo10,
        ReverseDirection,
        PlantBombRepeatedly,
    }

    public enum HeroEffectReason {
        Null,

        /** Uses booster. */
        UseBooster,

        /** Takes item. */
        TakeItem,

        /** Effect time-out. */
        TimeOut,

        /** Broken by bomb damage. */
        Damaged,

        /** Rescued by teammate. */
        Rescue,
    }

    public interface IHeroListener {
        void OnDamaged([NotNull] IHero hero, int amount, HeroDamageSource source);
        void OnHealthChanged([NotNull] IHero hero, int amount, int oldAmount);
        void OnItemChanged([NotNull] IHero hero, HeroItem item, int amount, int oldAmount);
        void OnEffectBegan([NotNull] IHero hero, HeroEffect effect, HeroEffectReason reason, int duration);
        void OnEffectEnded([NotNull] IHero hero, HeroEffect effect, HeroEffectReason reason);
        void OnMoved([NotNull] IHero hero, Vector2 position);
    }

    public interface IHero : IEntity {
        [NotNull]
        IHeroState State { get; }

        int Slot { get; }
        int TeamId { get; }
        HeroDamageSource DamageSource { get; }

        [NotNull]
        Dictionary<HeroItem, int> Items { get; }

        Vector2 Position { get; }
        Direction Direction { get; }

        void ApplyState([NotNull] IHeroState state);
        void Move(int timestamp, Vector2 position);
        IBomb PlantBomb(int timestamp, bool byHero);
        void DamageBomb(int amount);
        void DamagePrison();
        void RescuePrison();
        void DamageFallingBlock();
        void UseBooster(Booster booster);
        void TakeItem(BlockType blockType);
    }
}