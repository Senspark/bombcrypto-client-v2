using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public class StringMapPattern : IMapPattern {
        [NotNull]
        private readonly string _pattern;

        [NotNull]
        private readonly string[] _items;

        public int Width { get; }
        public int Height { get; }

        public StringMapPattern(
            [NotNull] string pattern
        ) {
            _pattern = pattern;
            _items = pattern
                .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Reverse()
                .ToArray();
            Width = _items.Max(it => it.Length);
            Height = _items.Length;
        }

        public char Get(Vector2Int position) {
            var row = _items[position.y];
            return row[position.x];
        }

        public List<Vector2Int> Find(Func<Vector2Int, char, bool> predicate) {
            var positions = new List<Vector2Int>();
            for (var y = 0; y < Height; ++y) {
                for (var x = 0; x < Width; ++x) {
                    var position = new Vector2Int(x, y);
                    var item = Get(position);
                    if (predicate(position, item)) {
                        positions.Add(position);
                    }
                }
            }
            return positions;
        }
    }
}