using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IUseEmojiData {
        [NotNull]
        string MatchId { get; }

        int Slot { get; }
        int ItemId { get; }
    }
}