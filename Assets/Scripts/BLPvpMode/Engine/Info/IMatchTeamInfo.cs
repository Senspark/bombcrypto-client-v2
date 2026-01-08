using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IMatchTeamInfo {
        [NotNull]
        int[] Slots { get; }
    }
}