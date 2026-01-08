using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Builders {
    public interface IMatchUserInfoBuilder {
        [NotNull]
        IMatchUserInfo Build();
    }
}