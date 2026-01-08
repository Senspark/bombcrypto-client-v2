using System.Collections.Generic;
using System.Threading.Tasks;

using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api.Modules;

using JetBrains.Annotations;

namespace BLPvpMode.Manager {
    public interface IUserFactory {
        [MustUseReturnValue]
        [NotNull]
        Task<(IUser[], List<IUserModule>)> Create([NotNull] IMatchInfo[] infoList);
    }
}