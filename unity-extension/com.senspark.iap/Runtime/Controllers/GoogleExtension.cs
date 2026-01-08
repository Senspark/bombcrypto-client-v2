using Cysharp.Threading.Tasks;

using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class GoogleExtension {
        public static UniTask ConfigGooglePlayService(ConfigurationBuilder builder) {
#if UNITY_ANDROID
            return ConfigGooglePlayServiceImpl(builder);
#endif
            return UniTask.CompletedTask;
        }
        
        private static async UniTask ConfigGooglePlayServiceImpl(ConfigurationBuilder builder) {
            var config = builder.Configure<IGooglePlayConfiguration>();

            config.SetServiceDisconnectAtInitializeListener(() => {
                FastLog.Error("[Senspark][IAP] User may not have a Google account on their device.");
            });

            config.SetQueryProductDetailsFailedListener((int retryCount) => {
                FastLog.Error("[Senspark][IAP] User may not enable internet connection.");
            });

            var adId = await Platform.GetAdvertisingId();
            if (!string.IsNullOrEmpty(adId)) {
                FastLog.Info("[Senspark][IAP] Set account id: " + adId);
                config.SetObfuscatedAccountId(adId);
            }
        }
    }
}