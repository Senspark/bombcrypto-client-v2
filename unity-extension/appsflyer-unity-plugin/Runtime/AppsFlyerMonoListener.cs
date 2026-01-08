using System;
using System.Collections.Generic;

using AppsFlyerConnector;

using AppsFlyerSDK;

using Manager;

using Newtonsoft.Json;

using UnityEngine;

namespace Senspark.Internal {
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// https://github.com/AppsFlyerSDK/appsflyer-unity-purchase-connector
    /// </summary>
    public class AppsFlyerMonoListener : MonoBehaviour, IAppsFlyerPurchaseValidation, IAppsFlyerConversionData {
        private const string LogTag = "[Senspark][AppsFlyer]";

        public Action<PurchaseValidatedData> OnPurchasedCallback;
        public Action<AdCampaignData> OnAdCampaignDataCallback;
        
        private AdCampaignData _adCampaignData;

        // Đây là hàm callback của AppsFlyer, ko được xóa hoặc đổi tên
        public void didReceivePurchaseRevenueValidationInfo(string validationInfo) {
            AppsFlyer.AFLog($"{LogTag} ValidationInfo", validationInfo);
#if UNITY_ANDROID
            AndroidValidate(validationInfo);
#elif UNITY_IOS
            IOSValidate(validationInfo);
#endif
        }

        /// https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/Assets/AppsFlyer/IAppsFlyerConversionData.cs
        /// https://support.appsflyer.com/hc/en-us/articles/360000726098-Conversion-Data-Scenarios#Introduction
        /// https://support.appsflyer.com/hc/en-us/articles/360001559405#testing-using-attribution-links
        public void onConversionDataSuccess(string conversionData) {
            /* Sample data:
             * Organic:
             * {"install_time":"2023-12-01 07:15:53.891","af_status":"Organic","af_message":"organic install","is_first_launch":true}
             * Non-organic:
             * {"adgroup_id":null,"retargeting_conversion_type":"none","is_incentivized":"false","orig_cost":"0.0","is_first_launch":true,
             * "af_cpi":null,"iscache":true,"click_time":"2023-12-01 07:52:52.341","match_type":"gp_referrer","campaign_id":null,
             * "install_time":"2023-12-01 07:56:47.364","media_source":"Test","advertising_id":"a5dc157a-be0c-48fb-ba02-16c7f4bb1896",
             * "af_status":"Non-organic","cost_cents_USD":"0","campaign":"GG_Campaign_US_IAP","http_referrer":null,"is_retargeting":"false","adgroup":null}
             */
            
            /* Cách test:
             Format URL: https://app.appsflyer.com/{0}?pid=Test&c={1}&advertising_id={2}
             {0} : android package name
             {1} : campaign name
             {2} : device advertising id
             Ví dụ vào URL: https://app.appsflyer.com/com.senspark.stickwar?pid=Test&c=GG_Campaign_US_IAP&advertising_id=a5dc157a-be0c-48fb-ba02-16c7f4bb1896
             Cài APK & run
             */
            
            if (conversionData.Length <= 200) {
                AppsFlyer.AFLog($"{LogTag} onConversionDataSuccess", conversionData);
            } else {
                // split conversionData into multiple string if too long
                var split = new List<string>();
                var i = 0;
                while (i < conversionData.Length) {
                    var length = Math.Min(200, conversionData.Length - i);
                    split.Add(conversionData.Substring(i, length));
                    i += length;
                }

                foreach (var s in split) {
                    AppsFlyer.AFLog($"{LogTag} onConversionDataSuccess", s);
                }
            }

            if (_adCampaignData != null) {
                return;
            }
            try {
                var data = JsonConvert.DeserializeObject<AdCampaignData>(conversionData);
                _adCampaignData = data;
                FastLog.Info($"{LogTag} CampaignInfo: {data.MediaSource} {data.Campaign} {data.CampaignId} {data.IsFirstLaunch} {data.AfStatus}");
                OnAdCampaignDataCallback?.Invoke(data);
            } catch (Exception e) {
                FastLog.Error($"{LogTag} onConversionDataSuccess error: {e.Message}");
            }
        }

        public void onConversionDataFail(string error) {
            AppsFlyer.AFLog($"{LogTag} onConversionDataFail", error);
        }

        /// https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#Intro
        public void onAppOpenAttribution(string attributionData) {
            AppsFlyer.AFLog($"{LogTag} onAppOpenAttribution", attributionData);
        }

        public void onAppOpenAttributionFailure(string error) {
            AppsFlyer.AFLog($"{LogTag} onAppOpenAttributionFailure", error);
        }

        // Đây là hàm callback của AppsFlyer, ko được xóa hoặc đổi tên
        public void didReceivePurchaseRevenueError(string error) {
            AppsFlyer.AFLog($"{LogTag} RevenueError", error);
        }

        private void AndroidValidate(string validationInfo) {
            try {
                var dictionary = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
                if (dictionary == null) {
                    return;
                }

                // if the platform is Android, you can create an object from the dictionnary 
                PurchaseValidatedData validatedData = null;
                if (dictionary.ContainsKey("productPurchase") && dictionary["productPurchase"] != null) {
                    var iapObject = JsonUtility.FromJson<InAppPurchaseValidationResult>(validationInfo);
                    var p = iapObject.productPurchase;
                    var isSuccess = iapObject.success && p.purchaseState == 0;
                    var isTestPurchase = p.purchaseType == 0;
                    validatedData = new PurchaseValidatedData(p.orderId, p.productId, isSuccess, isTestPurchase);
                } else if (dictionary.ContainsKey("subscriptionPurchase") &&
                           dictionary["subscriptionPurchase"] != null) {
                    var iapObject = JsonUtility.FromJson<SubscriptionValidationResult>(validationInfo);
                    var p = iapObject.subscriptionPurchase;
                    var isSuccess = iapObject.success && p.subscriptionState == "SUBSCRIPTION_STATE_ACTIVE";
                    var isTestPurchase = p.testPurchase != null;
                    validatedData = new PurchaseValidatedData(p.latestOrderId, p.lineItems[0].productId, isSuccess,
                        isTestPurchase);
                }
                if (validatedData == null) {
                    FastLog.Error($"{LogTag} Parsed failed");
                } else {
                    FastLog.Info(
                        $"{LogTag} ValidationInfo: {validatedData.ProductId} {validatedData.OrderId} {validatedData.IsSuccess} {validatedData.IsTestPurchase}");
                    OnPurchasedCallback?.Invoke(validatedData);
                }
            } catch (Exception e) {
                FastLog.Error($"{LogTag} ValidationInfo: {e.Message}");
            }
        }

        private void IOSValidate(string validationInfo) {
            // No way to know if it's a test purchase or not

            // try {
            //     var dictionary = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
            //     if (dictionary == null) {
            //         return;
            //     }
            //
            //     foreach (var (k,v) in dictionary) {
            //         
            //     }
            // } catch (Exception e) {
            //     FastLog.Error($"{LogTag} ValidationInfo: {e.Message}");
            // }
        }
    }
}