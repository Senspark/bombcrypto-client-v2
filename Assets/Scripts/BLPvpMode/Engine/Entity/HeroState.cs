using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public class HeroBaseState : IHeroBaseState {
        private static readonly (HeroItem, int)[] AllItems = {
            (HeroItem.BombUp, 4), // 
            (HeroItem.FireUp, 4), // 
            (HeroItem.Boots, 4), // 
            (HeroItem.Gold, 7), //
            (HeroItem.BronzeChest, 1), // 
            (HeroItem.SilverChest, 1), // 
            (HeroItem.GoldChest, 1), //
            (HeroItem.PlatinumChest, 1),
        };

        private static readonly HeroEffect[] AllEffects = {
            HeroEffect.Shield, //
            HeroEffect.Invincible, //
            HeroEffect.Imprisoned, // 
            HeroEffect.SpeedTo1, //
            HeroEffect.SpeedTo10, // 
            HeroEffect.ReverseDirection, //
            HeroEffect.PlantBombRepeatedly,
        };

        [NotNull]
        public static IHeroBaseState Decode([NotNull] long[] state) {
            var baseDecoder = new LongBitDecoder(state[0]);
            var itemDecoder = new LongBitDecoder(state[1]);
            var items = AllItems.Associate((item, maxBits) =>
                (item, itemDecoder.PopInt(maxBits))
            );
            var effects = AllEffects.AssociateIndex((item, index) =>
                (item, HeroEffectState.Decode(state[index + 2]))
            );
            return new HeroBaseState(
                isAlive: baseDecoder.PopBoolean(),
                health: baseDecoder.PopInt(4),
                damageSource: (HeroDamageSource) baseDecoder.PopInt(3),
                items: items,
                effects: effects
            );
        }

        public bool IsAlive { get; }
        public int Health { get; }
        public HeroDamageSource DamageSource { get; }
        public Dictionary<HeroItem, int> Items { get; }
        public Dictionary<HeroEffect, IHeroEffectState> Effects { get; }

        public HeroBaseState(
            bool isAlive,
            int health,
            HeroDamageSource damageSource,
            [NotNull] Dictionary<HeroItem, int> items,
            [NotNull] Dictionary<HeroEffect, IHeroEffectState> effects
        ) {
            IsAlive = isAlive;
            Health = health;
            DamageSource = damageSource;
            Items = items;
            Effects = effects;
        }

        public long[] Encode() {
            var items = new[] {
                new LongBitEncoder()
                    .Push(IsAlive)
                    .Push(Health, 4)
                    .Push((int) DamageSource, 3)
                    .Value,
                new LongBitEncoder().Let(it => {
                    AllItems.ForEach((item, maxBits) => {
                        var value = Items.TryGetValue(item, out var result) ? result : 0;
                        it.Push(value, maxBits);
                    });
                    return it; // Copy.
                }).Value,
            }.Concat(
                AllEffects.Select(effect => {
                    var effectState = Effects.TryGetValue(effect, out var value)
                        ? value
                        : new HeroEffectState(
                            isActive: false,
                            reason: HeroEffectReason.Null,
                            timestamp: 0,
                            duration: 0
                        );
                    return effectState.Encode();
                }).ToArray()
            ).ToArray();
            return items;
        }
    }

    public class HeroPositionState : IHeroPositionState {
        private const int PositionPrecision = 5;

        [NotNull]
        public static IHeroPositionState Decode(long state) {
            var decoder = new LongBitDecoder(state);
            return new HeroPositionState(
                position: new Vector2(
                    decoder.PopFloat(PositionPrecision, 21),
                    decoder.PopFloat(PositionPrecision, 21)
                ),
                direction: (Direction) decoder.PopInt(2)
            );
        }

        public Vector2 Position { get; }

        public Vector2Int PositionInt => new(
            Mathf.FloorToInt(Position.x),
            Mathf.FloorToInt(Position.y));

        public Direction Direction { get; }

        public HeroPositionState(
            Vector2 position,
            Direction direction
        ) {
            Position = position;
            Direction = direction;
        }

        public long Encode() {
            var encoder = new LongBitEncoder()
                .Push(Position.x, PositionPrecision, 21)
                .Push(Position.y, PositionPrecision, 21)
                .Push((int) Direction, 2);
            return encoder.Value;
        }
    }

    public class HeroState : IHeroState {
        public IHeroBaseState BaseState { get; }
        public IHeroPositionState PositionState { get; }
        public bool IsAlive { get; }

        public HeroState(
            [CanBeNull] IHeroBaseState baseState,
            [CanBeNull] IHeroPositionState positionState) {
            BaseState = baseState;
            PositionState = positionState;
            IsAlive = baseState?.IsAlive ?? false;
        }

        public HeroState(
            bool isAlive,
            Vector2 position,
            Direction direction,
            int health,
            HeroDamageSource damageSource,
            [NotNull] Dictionary<HeroItem, int> items,
            [NotNull] Dictionary<HeroEffect, IHeroEffectState> effects
        ) : this(
            new HeroBaseState(isAlive, health, damageSource, items, effects),
            new HeroPositionState(position, direction)
        ) { }
    }
}