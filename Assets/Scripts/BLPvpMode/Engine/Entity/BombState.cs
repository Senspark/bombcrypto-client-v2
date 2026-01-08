using System.Collections.Generic;

using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public class BombState : IBombState {
        private const int PositionPrecision = 5;

        private static readonly (Direction, int)[] AllDirections = {
            (Direction.Left, 4), //
            (Direction.Right, 4), // 
            (Direction.Up, 4), // 
            (Direction.Down, 4), // 
        };

        public static IBombState Decode(long[] state) {
            var baseDecoder = new LongBitDecoder(state[0]);
            var auxDecoder = new LongBitDecoder(state[1]);
            return new BombState(
                isAlive: baseDecoder.PopBoolean(),
                position: new Vector2(
                    baseDecoder.PopFloat(PositionPrecision, 21),
                    baseDecoder.PopFloat(PositionPrecision, 21)
                ),
                explodeRanges: AllDirections.Associate((item, maxBits) =>
                    (item, baseDecoder.PopInt(maxBits))
                ),
                reason: (BombReason) baseDecoder.PopInt(3),
                slot: baseDecoder.PopInt(2),
                plantTimestamp: auxDecoder.PopInt(20),
                explodeDuration: auxDecoder.PopInt(12),
                range: auxDecoder.PopInt(4),
                damage: auxDecoder.PopInt(4),
                piercing: auxDecoder.PopBoolean()
            );
        }

        public bool IsAlive { get; }
        public int Slot { get; }
        public BombReason Reason { get; }
        public Vector2 Position { get; }
        public int Range { get; }
        public int Damage { get; }
        public bool Piercing { get; }
        public int ExplodeDuration { get; }
        public Dictionary<Direction, int> ExplodeRanges { get; }
        public int PlantTimestamp { get; }

        public BombState(
            bool isAlive,
            int slot,
            BombReason reason,
            Vector2 position,
            int range,
            int damage,
            bool piercing,
            int explodeDuration,
            [NotNull] Dictionary<Direction, int> explodeRanges,
            int plantTimestamp
        ) {
            IsAlive = isAlive;
            Slot = slot;
            Reason = reason;
            Position = position;
            Range = range;
            Damage = damage;
            Piercing = piercing;
            ExplodeDuration = explodeDuration;
            ExplodeRanges = explodeRanges;
            PlantTimestamp = plantTimestamp;
        }

        public long[] Encode() {
            var items = new[] {
                new LongBitEncoder()
                    .Push(IsAlive)
                    .Push(Position.x, PositionPrecision, 21)
                    .Push(Position.y, PositionPrecision, 21)
                    .Let(it => {
                        AllDirections.ForEach((item, maxBits) => {
                            it.Push(ExplodeRanges.TryGetValue(item, out var value) ? value : 0, maxBits);
                        });
                        return it; // Copy.
                    })
                    .Push((int) Reason, 3)
                    .Push(Slot, 2)
                    .Value,
                new LongBitEncoder()
                    .Push(PlantTimestamp, 20)
                    .Push(ExplodeDuration, 12)
                    .Push(Range, 4)
                    .Push(Damage, 4)
                    .Push(Piercing)
                    .Value,
            };
            return items;
        }
    }
}