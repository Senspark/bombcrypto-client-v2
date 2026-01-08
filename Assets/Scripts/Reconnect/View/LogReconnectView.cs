using System.Threading.Tasks;

using JetBrains.Annotations;

using Senspark;

namespace Reconnect.View {
    public class LogReconnectView : IReconnectView {
        [NotNull]
        private readonly ILogManager _logManager;

        public LogReconnectView() {
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        }

        public Task StartReconnection() {
            _logManager.Log();
            return Task.CompletedTask;
        }

        public void UpdateProgress(int progress) {
            _logManager.Log($"Retry {progress}");
        }

        public Task FinishReconnection(bool successful) {
            _logManager.Log();
            return Task.CompletedTask;
        }

        public void KickByOtherDevice() {
            _logManager.Log();
        }

        public void FailToReconnect() {
            _logManager.Log();
        }
    }
}