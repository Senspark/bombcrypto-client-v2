using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.User {
    public class UserObserver {
        [CanBeNull]
        public Action<UserStatus> OnChangeStatus { get; set; }

        [CanBeNull]
        public Action OnStartMatch { get; set; }

        [CanBeNull]
        public Action<IPingPongData> OnPing { get; set; }

        [CanBeNull]
        public Action OnStartReady { get; set; }

        [CanBeNull]
        public Action<IMatchReadyData> OnReady { get; set; }

        [CanBeNull]
        public Action OnFinishReady { get; set; }

        [CanBeNull]
        public Action<IMatchStartData> OnStartRound { get; set; }

        [CanBeNull]
        public Action<IUseEmojiData> OnUseEmoji { get; set; }

        [CanBeNull]
        public Action<IFallingBlockData> OnFallingBlocks { get; set; }

        [CanBeNull]
        public Action<IMatchObserveData> OnChangeState { get; set; }

        [CanBeNull]
        public Action<IMatchFinishData> OnFinishRound { get; set; }

        [CanBeNull]
        public Action<IPvpResultInfo> OnFinishMatch { get; set; }
    }

    public enum UserStatus {
        Disconnected,
        Connecting,
        Connected,
    }

    public interface IUser : IObserverManager<UserObserver>, IDisposable {
        bool IsParticipant { get; }
        bool IsBot { get; }

        [NotNull]
        IMatchInfo MatchInfo { get; }

        UserStatus Status { get; }

        [MustUseReturnValue]
        [NotNull]
        Task Connect();

        [MustUseReturnValue]
        [NotNull]
        Task Disconnect();

        [MustUseReturnValue]
        [NotNull]
        Task KillConnection();

        [MustUseReturnValue]
        [NotNull]
        Task Ready();

        [MustUseReturnValue]
        [NotNull]
        Task Quit();

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IMoveHeroData> MoveHero(Vector2 position);

        [ItemNotNull]
        [MustUseReturnValue]
        [NotNull]
        Task<IPlantBombData> PlantBomb();

        [MustUseReturnValue]
        [NotNull]
        Task ThrowBomb();

        [MustUseReturnValue]
        [NotNull]
        Task UseBooster(Booster item);

        [MustUseReturnValue]
        [NotNull]
        Task UseEmoji(int itemId);
    }
}