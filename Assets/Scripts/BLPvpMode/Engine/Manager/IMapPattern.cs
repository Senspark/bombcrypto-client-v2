using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IMapPattern {
        int Width { get; }
        int Height { get; }
        char Get(Vector2Int position);

        [NotNull]
        List<Vector2Int> Find([NotNull] Func<Vector2Int, char, bool> predicate);
    }
}