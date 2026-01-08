using System.Threading.Tasks;

namespace Reconnect {
    public interface IReconnectView {
        Task StartReconnection();
        void UpdateProgress(int progress);
        Task FinishReconnection(bool successful);
        void KickByOtherDevice();
    }
}