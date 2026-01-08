using Senspark;

namespace Services {
    [Service(nameof(ILogoutManager))]
    public interface ILogoutManager {
        void Logout();
    }
}