using System;

using Services.IapAds;

using UnityEngine;

namespace Senspark.Ads {
    /// <summary>
    /// Add vào bằng gameObject.AddComponent
    /// </summary>
    public class RewardedAdButton : MonoBehaviour {
        private IUnityAdsManager _adsManager;
        private Action<bool> _onAdLoaded;
        private bool _isLoaded;
        private ObserverHandle _handle;

        public void Init(IUnityAdsManager adsManager, Action<bool> onAdLoaded) {
            _adsManager = adsManager;
            _onAdLoaded = onAdLoaded;
            _isLoaded = _adsManager.IsAdLoaded();
            _handle = new ObserverHandle();
            _handle?.AddObserver(_adsManager, new AdsManagerObserver {
                OnAdLoad = OnAdLoad
            });
            _onAdLoaded?.Invoke(_isLoaded);
        }

        public void Refresh() {
            _isLoaded = _adsManager.IsAdLoaded();
            _onAdLoaded?.Invoke(_isLoaded);
        }
        
        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void OnAdLoad(AdFormat format, bool loaded) {
            if (format != AdFormat.Rewarded) {
                return;
            }
            _isLoaded = loaded;
            _onAdLoaded?.Invoke(loaded);
        }
    }
}