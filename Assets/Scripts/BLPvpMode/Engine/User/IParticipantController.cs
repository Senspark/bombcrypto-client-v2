using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.User {
    public interface IParticipantController : IUserController {
        [NotNull]
        IMatchUserInfo Info { get; }

        int TeamId { get; }

        bool IsReady { get; }
        bool IsQuited { get; }
        void Ready();
        void Quit();
        void UseBooster(Booster booster);
        void Reset();
    }
}