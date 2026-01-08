using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.FallingBlock {
    public interface IFallingBlockGenerator {
        [ItemNotNull]
        IFallingBlockInfo[] Generate(int width, int height, int playTime);
    }
}