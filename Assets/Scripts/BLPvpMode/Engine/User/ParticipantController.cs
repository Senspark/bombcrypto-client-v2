using BLPvpMode.Engine.Info;
using BLPvpMode.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.User {
    public class ParticipantController : IParticipantController {
        public IUser User { get; private set; }

        public IMatchUserInfo Info { get; }
        public int TeamId { get; }
        public bool IsReady { get; private set; }

        public bool IsQuited { get; private set; }

        public ParticipantController(
            [NotNull] IMatchUserInfo info,
            int teamId,
            int slot,
            [NotNull] ITimeManager timeManager
        ) {
            Info = info;
            TeamId = teamId;
        }

        public void Join(IUser user) {
            User = user;
        }

        public void Leave() {
            User = null;
        }

        public void Ready() {
            IsReady = true;
        }

        public void Quit() {
            IsQuited = true;
        }

        public void UseBooster(Booster booster) {
            // FIXME.
        }

        public void Reset() {
            IsReady = false;
            IsQuited = false;
        }
    }
}