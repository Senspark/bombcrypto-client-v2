using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogWin : Dialog {
        public System.Action OnNewMapCallback;
        private bool _isClicked;
        private CancellationTokenSource _cancellation;

        public static UniTask<DialogWin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogWin>();

        }

        protected override void Awake() {
            base.Awake();
            _cancellation = new CancellationTokenSource();
            UniTask.Void(async (token) => {
                await WebGLTaskDelay.Instance.Delay(10 * 1000);
                NewMap();
            }, _cancellation.Token);
        }
        
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnNewMapClicked();
        }

        public void OnNewMapClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            NewMap();
        }

        private void NewMap() {
            if (_cancellation.IsCancellationRequested) {
                return;
            }

            _cancellation.Cancel();
            _cancellation.Dispose();
            OnNewMapCallback?.Invoke();
            Hide();
        }
    }
}