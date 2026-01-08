using System;

using App;

using BLPvpMode.Engine.User;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Manager.Api.Modules {
    public class AutoReadyModule : IUserModule {
        [NotNull]
        private readonly ObserverHandle _handle;

        private bool _disposed;

        public AutoReadyModule([NotNull] IUser user, float duration) {
            _handle = new ObserverHandle();
            _handle.AddObserver(user, new UserObserver {
                OnStartReady = () => {
                    UniTask.Void(async () => {
                        await WebGLTaskDelay.Instance.Delay((int) (duration * 1000));
                        try {
                            await user.Ready();
                        } catch (Exception ex) {
                            Debug.LogError(ex);
                        }
                    });
                },
            });
        }

        ~AutoReadyModule() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Update(float delta) { }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _handle.Dispose();
            }
            _disposed = true;
        }
    }
}