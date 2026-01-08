using System.Threading.Tasks;

using JetBrains.Annotations;

using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reconnect.View {
    public class LoadSceneReconnectView : IReconnectView {
        public static IReconnectView ToCurrentScene([NotNull] Canvas canvas) {
            return new LoadSceneReconnectView(canvas, SceneLoader.GetCurrentScene());
        }

        public static IReconnectView ToOtherScene([NotNull] Canvas canvas, [NotNull] string sceneName) {
            return new LoadSceneReconnectView(canvas, sceneName);
        }

        [NotNull]
        private readonly IReconnectView _waitingView;

        [CanBeNull]
        private readonly string _sceneName;

        private LoadSceneReconnectView(
            [NotNull] Canvas canvas,
            [CanBeNull] string sceneName
        ) {
            _waitingView = new WaitingReconnectView(canvas);
            _sceneName = sceneName;
        }

        public async Task StartReconnection() {
            await _waitingView.StartReconnection();
        }

        public void UpdateProgress(int progress) {
            _waitingView.UpdateProgress(progress);
        }

        public async Task FinishReconnection(bool successful) {
            await _waitingView.FinishReconnection(successful);
            if (successful) {
                if (_sceneName != null) {
                    if (SceneLoader.GetCurrentScene() == _sceneName) {
                        await SceneLoader.ReloadSceneAsync();
                    } else {
                        await SceneLoader.LoadSceneAsync(_sceneName);
                    }
                }
            }
        }

        public void KickByOtherDevice() {
            _waitingView.KickByOtherDevice();
        }
    }
}