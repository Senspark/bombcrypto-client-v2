using JetBrains.Annotations;

namespace BLPvpMode.Engine.Delta {
    public interface IMatchStateDelta {
        [NotNull]
        IHeroStateDelta[] Hero { get; }

        [NotNull]
        IBombStateDelta[] Bomb { get; }

        [NotNull]
        IBlockStateDelta[] Block { get; }
    }
}