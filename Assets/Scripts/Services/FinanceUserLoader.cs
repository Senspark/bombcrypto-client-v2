using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Services.UserLoader;

namespace Services {
    public class FinanceUserLoader : IFinanceUserLoader {
        private UniTaskCompletionSource _tcs;
        private readonly IUserLoader _userLoaderBridge = new DefaultUserLoader();

        public void Destroy() {
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public async UniTask LoadAsync(Action<int, string> updateProgress = null) {
            if (_tcs != null) {
                await _tcs.Task;
                return;
            }
            
            var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _tcs = new UniTaskCompletionSource();
            try {
                var allLoads = _userLoaderBridge.GetLoads();
                var progress = 80;
                foreach (var (title, load) in allLoads) {
                    progress += 2;
                    progress = Math.Min(progress, 98);
                    if (updateProgress != null) {
                        updateProgress(progress, title);
                    }
                    logManager.Log(title);
                    await load();
                }
                _tcs.TrySetResult();
            } catch (Exception e) {
                logManager.Log(e.Message);
                _tcs.TrySetException(e);
                _tcs = null;
                throw;
            }
        }
    }
}