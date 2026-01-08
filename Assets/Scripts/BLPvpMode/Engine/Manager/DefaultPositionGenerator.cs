using System.Linq;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public class DefaultPositionGenerator : IPositionGenerator {
        [NotNull]
        private readonly string _chars;

        public DefaultPositionGenerator(
            [NotNull] string chars
        ) {
            _chars = chars;
        }

        public Vector2Int[] Generate(IMapPattern pattern) {
            return _chars
                .SelectMany(it => pattern.Find((_, c) => c == it))
                .ToArray();
        }
    }
}