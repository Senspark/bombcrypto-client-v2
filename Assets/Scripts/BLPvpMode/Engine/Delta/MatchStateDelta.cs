using JetBrains.Annotations;

namespace BLPvpMode.Engine.Delta {
    public class MatchStateDelta : IMatchStateDelta {
        public IHeroStateDelta[] Hero { get; }
        public IBombStateDelta[] Bomb { get; }
        public IBlockStateDelta[] Block { get; }

        public MatchStateDelta(
            [NotNull] IHeroStateDelta[] hero,
            [NotNull] IBombStateDelta[] bomb,
            [NotNull] IBlockStateDelta[] block) {
            Hero = hero;
            Bomb = bomb;
            Block = block;
        }
    }
}