using System.Threading.Tasks;

using App;

namespace Analytics {
    public class DefaultAnalyticsDependency : IAnalyticsDependency {
        private readonly IUserAccountManager _userAccountManager;

        public DefaultAnalyticsDependency(IUserAccountManager userAccountManager) {
            _userAccountManager = userAccountManager;
        }

        public int GetUserId() {
            var acc = _userAccountManager.GetRememberedAccount();
            return acc?.id ?? -1;
        }
    }

    public class NullAnalyticsDependency : IAnalyticsDependency {
        public int GetUserId() {
            return -1;
        }
    }
}