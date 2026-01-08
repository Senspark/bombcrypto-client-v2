using System;
using System.Threading;

using AppsFlyerConnector;

using AppsFlyerSDK;

using Cysharp.Threading.Tasks;

using Manager;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Senspark.Internal {
    internal class AppsFlyerValidatorBridge : ObserverManager<RevenueValidatorObserver>, IRevenueValidator {
        private const string KTag = "[Senspark][ValidatorBridge]";

        public PurchaseValidatedData LastPurchaseValidatedData { get; private set; }
        public AdCampaignData LastAdCampaignData { get; private set; }
        
        private AppsFlyerMonoListener _appsFlyerGo;

        public AppsFlyerValidatorBridge() {
            var go = new GameObject(nameof(AppsFlyerBridge));
            Object.DontDestroyOnLoad(go);
            _appsFlyerGo = go.AddComponent<AppsFlyerMonoListener>();
            _appsFlyerGo.OnPurchasedCallback = OnPurchased;
            _appsFlyerGo.OnAdCampaignDataCallback = OnAdCampaignDataCallback;

            Initialize().Forget();
        }

        private async UniTaskVoid Initialize() {
            if (!await WaitForAppsFlyer()) {
                return;
            }
            AppsFlyer.getConversionData(_appsFlyerGo.name);

            AppsFlyerPurchaseConnector.init(_appsFlyerGo, Store.GOOGLE);
            AppsFlyerPurchaseConnector.setIsSandbox(false);
            AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
            AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
            AppsFlyerPurchaseConnector.build();
            AppsFlyerPurchaseConnector.startObservingTransactions();
        }

        public void Dispose() {
            if (_appsFlyerGo == null) {
                return;
            }
            _appsFlyerGo.OnPurchasedCallback = null;
            _appsFlyerGo.OnAdCampaignDataCallback = null;

            Object.Destroy(_appsFlyerGo.gameObject);
            _appsFlyerGo = null;
        }

        private static async UniTask<bool> WaitForAppsFlyer() {
            var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            var timeOutTask = UniTask.Delay(TimeSpan.FromSeconds(30), false, PlayerLoopTiming.Update, token);
            var appsFlyerTask = UniTask.WaitUntil(() => AppsFlyer.instance != null && AppsFlyer.instance.isInit,
                PlayerLoopTiming.Update, token);
            var completedTaskIndex = await UniTask.WhenAny(timeOutTask, appsFlyerTask);
            var success = completedTaskIndex == 1;
            cancellation.Cancel();
            return success;
        }

        private void OnPurchased(PurchaseValidatedData data) {
            Log($"Received purchased data: {data.ProductId} - {data.OrderId}");
            LastPurchaseValidatedData = data;
            DispatchEvent(e => e?.OnIapRevenueValidated?.Invoke(data));
        }

        private void OnAdCampaignDataCallback(AdCampaignData data) {
            Log($"Received campaign data: {data.Campaign} - {data.AfStatus}");
            LastAdCampaignData = data;
            DispatchEvent(e => e?.OnAdCampaignDataReceived?.Invoke(data));
        }

        private void Log(string message) {
            FastLog.Info($"{KTag} {message}");
        }
    }
}