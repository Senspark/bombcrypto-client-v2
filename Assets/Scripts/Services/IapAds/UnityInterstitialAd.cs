using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using GoogleMobileAds.Api;

using UnityEngine;

namespace Services.IapAds {
    public class UnityInterstitialAd {
#if UNITY_ANDROID
        private string INTERSTITIAL_AD_ID_ADMOB => AppConfig.AdmobInterstitialAdIdAndroid;
#elif UNITY_IOS
        private string INTERSTITIAL_AD_ID_ADMOB => AppConfig.AdmobInterstitialAdIdIos;
#else
        private string INTERSTITIAL_AD_ID_ADMOB => AppConfig.AdmobInterstitialAdIdTest;
#endif
        
        private readonly ILogManager _logManager;
        private InterstitialAd _ad;
        
        private AdLoadState _adLoadState = AdLoadState.NotLoad;
        private AdViewState _adViewState = AdViewState.NotStart;
        
        public UnityInterstitialAd(ILogManager logManager) {
            _logManager = logManager;
        }
        
        public void CreateNewAd() {
            if (_adLoadState != AdLoadState.NotLoad) {
                return;
            }
            _adLoadState = AdLoadState.Loading;
            
            DestroyOldAd();
            var adRequest = new AdRequest.Builder().Build();
            InterstitialAd.Load(INTERSTITIAL_AD_ID_ADMOB, adRequest, (ad, error) => {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.Log($"devv Interstitial failed to load: {error}");
                    _adLoadState = AdLoadState.NotLoad;
                    DestroyOldAd();
                    return;
                }
                Debug.Log("devv Interstitial loaded");
                _logManager.Log($"{ad.GetResponseInfo()}");
                _adLoadState = AdLoadState.Loaded;
                _ad = ad;
                _ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
                _ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
            });
        }

        public async Task<AdResult> ShowAd() {
            switch (_adLoadState) {
                case AdLoadState.NotLoad: {
                    _logManager.Log($"devv ads fail to load. Create new ad");
                    CreateNewAd();
                    return AdResult.Error;
                }
                case AdLoadState.Loading: {
                    _logManager.Log($"devv ads still loading.");
                    return AdResult.Error;
                }
                case AdLoadState.Loaded: {
                    if (!_ad.CanShowAd()) {
                        _logManager.Log($"devv ads cannot show. Create new ad");
                        CreateNewAd();
                        return AdResult.Error;
                    }
                    // will show ad
                    break;
                }
                default: {
                    _logManager.Log($"devv AdResult not specify");
                    return AdResult.Error;
                }
            }
            
            if (_adViewState == AdViewState.NotStart) {
                _adViewState = AdViewState.IsViewing;
                _ad.Show();
            }
            await UniTask.WaitUntil(() => _adViewState != AdViewState.IsViewing);
            var result = _adViewState switch {
                AdViewState.Done => AdResult.Done,
                _ => AdResult.Error
            };
            _adViewState = AdViewState.NotStart;
            _adLoadState = AdLoadState.NotLoad;
            CreateNewAd();
            return result;
        }
        
        private void DestroyOldAd() {
            if (_ad == null) {
                return;
            }
            _ad.OnAdFullScreenContentClosed -= OnAdFullScreenContentClosed;
            _ad.OnAdFullScreenContentFailed -= OnAdFullScreenContentFailed;
            _ad.Destroy();
            _ad = null;
        }
        
        private void OnAdFullScreenContentFailed(AdError err) {
            Debug.Log($"devv {err.GetCode()} {err.GetMessage()}");
            _adViewState = AdViewState.Error;
        }

        private void OnAdFullScreenContentClosed() {
            _logManager.Log();
            _adViewState = AdViewState.Done;
        }
    }
}