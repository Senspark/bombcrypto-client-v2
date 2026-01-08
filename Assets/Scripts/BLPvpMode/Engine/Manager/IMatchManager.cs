using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IMatchManager {
        void Validate([NotNull] IMatchInfo info, [NotNull] string hash);

        [NotNull]
        IMatchController Join([NotNull] IUser user);

        void Leave([NotNull] IUser user);
        void Finish([NotNull] IPvpResultInfo resultInfo);
    }
}