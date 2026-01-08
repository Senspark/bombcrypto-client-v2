using System.Threading.Tasks;

using Exceptions;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public interface IGameReadyController {
        /// <summary>
        /// Init tất cả service & account cần thiết để run game
        /// </summary>
        /// <exception cref="ServerMaintenanceException"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="LoginException"></exception>
        /// <exception cref="NoInternetException"></exception>
        Task Start(int progressEnd, bool forceToMainMenu = false, bool isForceLogin = false);
        void ResetProgress();

        void Cancel();
    }
    
    public readonly struct GameReadyProgress {
        public readonly int Progress;
        public readonly string Details;

        public GameReadyProgress(int progress, string details) {
            Progress = progress;
            Details = details;
        }
    }
}