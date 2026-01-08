using System.Collections.Generic;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface ICommandManager {
        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IMoveHeroData> MoveHero(int slot, int timestamp, Vector2 position);

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IPlantBombData> PlantBomb(int slot, int timestamp);

        [MustUseReturnValue]
        [NotNull]
        Task ThrowBomb(int slot, int timestamp);

        [MustUseReturnValue]
        [NotNull]
        Task UseBooster(int slot, int timestamp, Booster item);

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        List<IMatchObserveData> ProcessCommands();
    }
}