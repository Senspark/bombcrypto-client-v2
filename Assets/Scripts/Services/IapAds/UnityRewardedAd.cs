using System.Threading.Tasks;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using GoogleMobileAds.Api;

using UnityEngine;

namespace Services.IapAds {
    public class UnityRewardedAd : ObserverManager<AdsManagerObserver>  {
#if UNITY_ANDROID
        private string REWARDED_AD_ID_ADMOB => AppConfig.AdmobRewardedAdIdAndroid;
#elif UNITY_IOS
        private string REWARDED_AD_ID_ADMOB => AppConfig.AdmobRewardedAdIdIos;
#else
        private string REWARDED_AD_ID_ADMOB => AppConfig.AdmobRewardedAdIdTest;
#endif

        private readonly ILogManager _logManager;
        private readonly IAnalytics _analytics;

        private RewardedAd _ad;
        private AdLoadState _adLoadState = AdLoadState.NotLoad;
        private AdViewState _adViewState = AdViewState.NotStart;

        public UnityRewardedAd(ILogManager logManager, IAnalytics analytics) {
            _logManager = logManager;
            _analytics = analytics;
        }

        public bool IsAdLoaded() {
            // Nếu khi kiểm tra mà trạng thái là NotLoad thi reload để cập nhật trạng thái.
            if (_adLoadState == AdLoadState.NotLoad) {
                CreateNewAd();
            }
            return _adLoadState == AdLoadState.Loaded;
        }
        
        public void CreateNewAd() {
            if (_adLoadState != AdLoadState.NotLoad) {
                return;
            }
            _adLoadState = AdLoadState.Loading;

            // Clean up the old ad before loading a new one.
            DestroyOldAd();

            // Load new ad
            Debug.Log($"devv Loading new ad");
            var adRequest = new AdRequest.Builder().Build();
            RewardedAd.Load(REWARDED_AD_ID_ADMOB, adRequest, (ad, error) => {
                // if error is not null, the load request failed.
                if (error != null || ad == null) {
                    Debug.Log($"devv Rewarded failed to load: {error}");
                    _adLoadState = AdLoadState.NotLoad;
                    DestroyOldAd();
                    DispatchEvent(e => e.OnAdLoad?.Invoke(AdFormat.Rewarded, false));
                    return;
                }
                Debug.Log($"devv Rewarded ad loaded");
                _logManager.Log($"{ad.GetResponseInfo()}");
                _ad = ad;
                _ad.OnAdPaid += OnAdPaid;
                _ad.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
                _ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
                _ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
                _adLoadState = AdLoadState.Loaded;
                DispatchEvent(e => e.OnAdLoad?.Invoke(AdFormat.Rewarded, true));
            });
        }
        
        public async Task<AdResult> ShowAd(string ssvId) {
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
                var options = new ServerSideVerificationOptions.Builder().SetUserId(ssvId).Build();
                _logManager.Log($"devv ads ssv Id: {ssvId}");
                _ad.SetServerSideVerificationOptions(options);
                _ad.Show(_ => {
                    _logManager.Log("devv Rewarded ad done - not closed yet");
                    _adViewState = AdViewState.Done;
                });
            }
            await UniTask.WaitUntil(() => _adViewState != AdViewState.IsViewing);
            var result = _adViewState switch {
                AdViewState.Cancel => AdResult.Cancel,
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
            _ad.OnAdPaid -= OnAdPaid;
            _ad.OnAdFullScreenContentOpened -= OnAdFullScreenContentOpened;
            _ad.OnAdFullScreenContentClosed -= OnAdFullScreenContentClosed;
            _ad.OnAdFullScreenContentFailed -= OnAdFullScreenContentFailed;
            _ad.Destroy();
            _ad = null;
        }

        private void OnAdPaid(AdValue obj) {
            _logManager.Log();
            var info = _ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
            var value = obj.Value;
            var currencyCode = obj.CurrencyCode;
            var adSourceName = info?.AdSourceName ?? "";
            _analytics?.LogAdRevenue(
                AdNetwork.AdMob,
                adSourceName,
                value / 1e6f,
                currencyCode,
                AdFormat.Rewarded,
                REWARDED_AD_ID_ADMOB
            );
        }

        private void OnAdFullScreenContentOpened() {
            _logManager.Log();
            // ignore
        }

        private void OnAdFullScreenContentClosed() {
            _logManager.Log();
            if (_adViewState == AdViewState.IsViewing) {
                _adViewState = AdViewState.Cancel;
            }
        }

        private void OnAdFullScreenContentFailed(AdError err) {
            Debug.Log($"devv {err.GetCode()} {err.GetMessage()}");
            _adViewState = AdViewState.Error;
        }
    }
}