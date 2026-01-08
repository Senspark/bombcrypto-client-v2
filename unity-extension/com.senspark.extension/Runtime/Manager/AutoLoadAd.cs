using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    public class AutoLoadAd : IAd {
        [NotNull]
        private readonly IAd _ad;

        [CanBeNull]
        private Task _loadTask;

        public bool IsLoaded => _ad.IsLoaded;

        public AutoLoadAd([NotNull] IAd ad) {
            _ad = ad;
        }

        public int AddObserver(AdObserver observer) {
            return _ad.AddObserver(observer);
        }

        public bool RemoveObserver(int id) {
            return _ad.RemoveObserver(id);
        }

        public void DispatchEvent(Action<AdObserver> dispatcher) {
            _ad.DispatchEvent(dispatcher);
        }

        public void Dispose() {
            _ad.Dispose();
        }

        public void Initialize() {
            _ad.Initialize();
            _ = Load();
        }

        public async Task<bool> Load() {
            if (_loadTask != null) {
                await _loadTask;
                return true;
            }
            try {
                _loadTask = LoadImpl();
                await _loadTask;
                return true;
            } finally {
                _loadTask = null;
            }
        }

        [NotNull]
        private async Task LoadImpl() {
            var retries = 0;
            while (true) {
                var result = await _ad.Load();
                if (result) {
                    return;
                }
                ++retries;
                var delay = 1000 * (1 << Mathf.Min(6, retries));
                await Task.Delay(delay);
            }
        }
    }
}