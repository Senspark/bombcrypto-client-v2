using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

namespace Game.Dialog.Connects {
    public abstract class AfDialog<T> : Dialog, IAccountFlowState {
        private ISoundManager _soundManager;
        private WaitingUiManager _waiting;
        protected AccountFlowData Data { get; private set; }
        private UniTaskCompletionSource<T> _task;

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            OnDidHide(Reject);
        }

        protected void PlayClickSound() {
            _soundManager.PlaySound(Audio.Tap);
        }

        protected void Resolve(T value) {
            _task?.TrySetResult(value);
        }

        protected void Reject() {
            _task?.TrySetCanceled();
        }

        protected void ShowWaiting() {
            if (_waiting != null) {
                return;
            }
            _waiting = new WaitingUiManager(Data.CurrentCanvas);
            _waiting.Begin();
        }

        protected void HideWaiting() {
            if (_waiting == null) {
                return;
            }
            _waiting.End();
            _waiting = null;
        }

        public async UniTask<StateReturnValue<T1>> StartFlow<T1>(AccountFlowData data) {
            _task = new UniTaskCompletionSource<T>();
            try {
                Data = data;
                Show(data.CurrentCanvas);
                var r1 = await _task.Task;
                Hide();
                var r2 = new StateReturnValue<T1>(false, (T1)Convert.ChangeType(r1, typeof(T1)));
                return r2;
            } catch (Exception _) {
                Hide();
                return new StateReturnValue<T1>(true, default);
            }
        }
    }
}