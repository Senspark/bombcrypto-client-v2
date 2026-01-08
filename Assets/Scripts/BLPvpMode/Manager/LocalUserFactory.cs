using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api;
using BLPvpMode.Manager.Api.Modules;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Manager {
    public class LocalUserFactory : IUserFactory {
        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly IMatchManager _matchManager;

        public LocalUserFactory() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _timeManager = new EpochTimeManager();
            _matchManager = new PvpMatchManager(_logManager);
        }

        public async Task<(IUser[], List<IUserModule>)> Create(IMatchInfo[] infoList) {
            var modules = new List<IUserModule>();
            var users = infoList
                .Select(info => {
                    var user = (IUser) new LocalUser(info, _matchManager, _timeManager);
                    if (user.IsParticipant) {
                        if (user.IsBot) {
                            modules.Add(new AutoReadyModule(user, 2f));
                        }
                    }
                    return user;
                })
                .ToArray();
            return (users, modules);
        }
    }
}