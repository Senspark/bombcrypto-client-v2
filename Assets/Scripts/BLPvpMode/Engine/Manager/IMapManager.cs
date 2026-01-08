using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IMapListener {
        void OnAdded([NotNull] IBlock block, BlockReason reason);
        void OnRemoved([NotNull] IBlock block, BlockReason reason);
    }

    public interface IMapManagerState {
        bool CanDropChestBlock { get; }

        [NotNull]
        Dictionary<Vector2Int, IBlockState> Blocks { get; }

        [NotNull]
        IMapManagerState Apply([NotNull] IMapManagerState state);

        long[] Encode();
    }

    public interface IMapManager {
        int Tileset { get; }
        int Width { get; }
        int Height { get; }
        bool CanDropChestBlock { get; }

        [NotNull]
        IMapManagerState State { get; }

        void ApplyState([NotNull] IMapManagerState state);

        [CanBeNull]
        IBlock GetBlock(Vector2Int position);

        void AddBlock([NotNull] IBlock block);

        void RemoveBlock([NotNull] IBlock block);
    }
}