namespace BLPvpMode.Engine.Config {
    public class ConstantMapConfig : IMapConfig {
        public int PlayTime => 120000;

        public int[] TilesetList => new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, };
        public float BlockDensity => 0.5f;
        public float ItemDensity => 0.5f;

        public FallingBlockPattern[] FallingBlockPatternList => new[] {
            FallingBlockPattern.TopLeftCw, //
            FallingBlockPattern.TopLeftCcw, //
            FallingBlockPattern.BottomRightCw, //
            FallingBlockPattern.BottomRightCcw, // 
            FallingBlockPattern.TopLeftDualCw, // 
            FallingBlockPattern.TopLeftDualCcw,
        };

        public int ExplodeDuration => 3000;
    }
}