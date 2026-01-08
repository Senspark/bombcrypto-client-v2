using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IMatchController {
        void Initialize();
        void JoinRoom([NotNull] IUser user);
        void LeaveRoom([NotNull] IUser user);
        void Ready([NotNull] IUser user);
        void Quit([NotNull] IUser user);

        void Ping([NotNull] IUser user, long timestamp, int requestId);

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IMoveHeroData> MoveHero([NotNull] IUser user, long timestamp, Vector2 position);

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IPlantBombData> PlantBomb([NotNull] IUser user, long timestamp);

        [MustUseReturnValue]
        [NotNull]
        Task ThrowBomb([NotNull] IUser user, long timestamp);

        [MustUseReturnValue]
        [NotNull]
        Task UseBooster([NotNull] IUser user, long timestamp, Booster item);

        void UseEmoji([NotNull] IUser user, int itemId);
    }
}