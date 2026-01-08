using JetBrains.Annotations;

namespace BLPvpMode.Engine.Config {
    public interface IMapConfig {
        int PlayTime { get; }

        [NotNull]
        int[] TilesetList { get; }

        float BlockDensity { get; }
        float ItemDensity { get; }
        FallingBlockPattern[] FallingBlockPatternList { get; }
        int ExplodeDuration { get; }
    }
}