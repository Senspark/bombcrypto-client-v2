using JetBrains.Annotations;

namespace BLPvpMode.Engine.User {
    public interface IUserController {
        [CanBeNull]
        IUser User { get; }

        void Join([NotNull] IUser user);
        void Leave();
    }
}