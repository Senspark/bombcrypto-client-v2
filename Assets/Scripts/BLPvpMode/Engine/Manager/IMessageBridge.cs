using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IMessageBridge {
        void StartMatch([NotNull] IUser[] users);
        void Ping([NotNull] IPingPongData data, [NotNull] IUser[] users);
        void StartReady([NotNull] IUser[] users);
        void Ready([NotNull] IMatchReadyData data, [NotNull] IUser[] users);
        void FinishReady([NotNull] IUser[] users);
        void StartRound([NotNull] IMatchStartData data, [NotNull] IUser[] users);
        void UseEmoji([NotNull] IUseEmojiData data, [NotNull] IUser[] users);
        void BufferFallingBlocks([NotNull] IFallingBlockData data, [NotNull] IUser[] users);
        void ChangeState([NotNull] IMatchObserveData data, [NotNull] IUser[] users);
        void FinishRound([NotNull] IMatchFinishData data, [NotNull] IUser[] users);
        void FinishMatch([NotNull] IPvpResultInfo data, [NotNull] IUser[] users);
    }
}