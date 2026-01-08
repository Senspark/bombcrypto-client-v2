using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Share.Scripts.Dialog;
using Share.Scripts.Services;

using UnityEngine;

namespace Game.Manager {
    public class WaitingUiManager {
        public static bool Enable { set; get; }

        private readonly Canvas _canvasDialog;
        private readonly AsyncWrapper<DialogWaiting> _waiting;
        private readonly CancellationTokenSource _cancellation;

        public WaitingUiManager(Canvas canvasDialog) {
            _canvasDialog = canvasDialog;
            _waiting = new AsyncWrapper<DialogWaiting>(DialogWaiting.Create());
            _cancellation = new CancellationTokenSource();
        }

        // public WaitingUiManager OnWillShow(Action action) {
        //     _waiting.OnWillShow(action);
        //     return this;
        // }

        public WaitingUiManager OnDidShow(Action action) {
            _waiting.Call(e => e.OnDidShow(action));
            return this;
        }

        public WaitingUiManager OnWillHide(Action action) {
            _waiting.Call(e => e.OnWillHide(action));
            return this;
        }

        public WaitingUiManager OnDidHide(Action action) {
            _waiting.Call(e => e.OnDidHide(action));
            return this;
        }

        public void ChangeText(string text) {
            _waiting.Call(e => e.ChangeText(text));
        }

        public void Begin(float delay = 0f) {
            _waiting.Call(e => e.BlockUi(_canvasDialog));
            if (!Enable) {
                return;
            }
            if (delay <= 0) {
                if (_cancellation.IsCancellationRequested) {
                    return;
                }
                _waiting.Call(e => e.Show(_canvasDialog));
                return;
            }

            var cancel = _cancellation.Token;
            UniTask.Void(async c => {
                var iDelay = (int)(delay * 1000);
                await UniTask.Delay(iDelay, cancellationToken: c);
                if (!_cancellation.IsCancellationRequested) {
                    _waiting.Call(e => e.Show(_canvasDialog));
                }
            }, cancel);
        }

        public void End() {
            Cancel();
            _cancellation.Dispose();
        }

        private void Cancel() {
            if (_cancellation.IsCancellationRequested) {
                return;
            }
            _cancellation.Cancel();
            _waiting.Call(e => e.Hide());
        }
    }
}