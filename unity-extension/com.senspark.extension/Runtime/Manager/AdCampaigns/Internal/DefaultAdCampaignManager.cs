using System;
using System.Collections.Generic;

using Manager;

namespace Senspark.AdCampaigns.Internal {
    internal class DefaultAdCampaignManager : IAdCampaignManager {
        private readonly IRevenueValidator _revenueValidator;
        private readonly IRemoteConfigManager _remoteConfig;
        private readonly IAnalyticsManager _analytics;
        private readonly IDataManager _dataManager;
        private readonly ObserverHandle _handle;

        private const string KTag = "[Senspark][AdCampaign]";
        private const string KCampaignRemoteKey = "campaign_iap";
        private const string KAnalyticsKey = "conv_ad_campaign";
        private const string KSaltCampaignType = "_iap";
        private const string KSaltCampaignName = " - 2023 VN";
        private readonly string _kDataKeyCampaignType = "campaign";
        private readonly string _kDataKeyCampaignName = "campaign_name";

        public AdCampaignType CampaignType { get; private set; }

        public DefaultAdCampaignManager(
            IRevenueValidator revenueValidator,
            IRemoteConfigManager remoteConfig,
            IAnalyticsManager analytics,
            IDataManager dataManager
        ) {
            _revenueValidator = revenueValidator;
            _remoteConfig = remoteConfig;
            _analytics = analytics;
            _dataManager = dataManager;

            _kDataKeyCampaignType = Security.Encryption.Obfuscate(_kDataKeyCampaignType);
            _kDataKeyCampaignName = Security.Encryption.Obfuscate(_kDataKeyCampaignName);

            CampaignType = LoadCampaignType();
            switch (CampaignType) {
                case AdCampaignType.Unknown:
                    // Chưa xử lý lần nào thì mới listen
                    _handle = new ObserverHandle();
                    _handle.AddObserver(revenueValidator, new RevenueValidatorObserver {
                        OnAdCampaignDataReceived = OnAdCampaignDataReceived
                    });
                    break;
                case AdCampaignType.None:
                    // Organic thì ko cần xử lý làm gì nữa
                    break;
                default:
                    //  Check campaign type lần nữa vì có thể remote config đã bổ sung hoặc gỡ bỏ vài campaign
                    RecheckCampaignType();
                    break;
            }
        }

        public void Dispose() {
            _revenueValidator?.Dispose();
            _handle?.Dispose();
        }

        private void OnAdCampaignDataReceived(AdCampaignData data) {
            if (!IsValidCampaignName(data.Campaign)) {
                SaveCampaignType(AdCampaignType.None);
                return;
            }
            try {
                var type = IsInRemoteCampaigns(data.Campaign) ? AdCampaignType.Iap : AdCampaignType.General;
                SaveCampaignType(type);
                SaveCampaignName(data.Campaign);
                _analytics.LogEvent(KAnalyticsKey, new Dictionary<string, object> {
                    { "campaign_name", data.Campaign ?? string.Empty },
                    { "campaign_id", data.CampaignId ?? string.Empty },
                    { "campaign_type", ConvertFrom(type) ?? string.Empty },
                    { "media_source", data.MediaSource ?? string.Empty },
                    { "is_first_launch", data.IsFirstLaunch }
                });
            } catch (Exception e) {
                FastLog.Error($"{KTag} Error: {e.Message}");
            }
        }

        private void RecheckCampaignType() {
            var campaignName = LoadCampaignName();
            if (!IsValidCampaignName(campaignName)) {
                return;
            }
            var campaignType = IsInRemoteCampaigns(campaignName) ? AdCampaignType.Iap : AdCampaignType.General;
            if (campaignType != CampaignType) {
                SaveCampaignType(campaignType);
                _analytics.LogEvent(KAnalyticsKey, new Dictionary<string, object> {
                    { "campaign_name", campaignName },
                    { "campaign_type", ConvertFrom(campaignType) },
                    { "is_first_launch", false }
                });
            }
        }

        private void SaveCampaignType(AdCampaignType type) {
            CampaignType = type;
            var campaignType = ConvertFrom(type);
            var obsCampaignType = Security.Encryption.Obfuscate(campaignType + KSaltCampaignType);
            _dataManager.Set(_kDataKeyCampaignType, obsCampaignType);
            FastLog.Info($"{KTag} scamp {campaignType}s");
        }

        private void SaveCampaignName(string campaignName) {
            var obsCampaignName = Security.Encryption.Obfuscate(campaignName + KSaltCampaignName);
            _dataManager.Set(_kDataKeyCampaignName, obsCampaignName);
            FastLog.Info($"{KTag} scamp #2");
        }

        private AdCampaignType LoadCampaignType() {
            var campaignType = _dataManager.Get(_kDataKeyCampaignType, string.Empty);
            var deObsCampaignType = Security.Encryption.DeObfuscate(campaignType).Replace(KSaltCampaignType, string.Empty);
            FastLog.Info($"{KTag} lcamp {deObsCampaignType}s");
            return ConvertFrom(deObsCampaignType);
        }

        private string LoadCampaignName() {
            var campaignName = _dataManager.Get(_kDataKeyCampaignName, string.Empty);
            var deObsCampaignName = Security.Encryption.DeObfuscate(campaignName).Replace(KSaltCampaignName, string.Empty);;
            FastLog.Info($"{KTag} lcamp #2");
            return deObsCampaignName;
        }

        private static string ConvertFrom(AdCampaignType type) {
            return type switch {
                AdCampaignType.Unknown => "u",
                AdCampaignType.None => "n",
                AdCampaignType.General => "g",
                AdCampaignType.Iap => "i",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private static AdCampaignType ConvertFrom(string str) {
            return str switch {
                "i" => AdCampaignType.Iap,
                "g" => AdCampaignType.General,
                "n" => AdCampaignType.None,
                _ => AdCampaignType.Unknown
            };
        }

        private bool IsInRemoteCampaigns(string curCampaign) {
            if (!IsValidCampaignName(curCampaign)) {
                // Tên campaign ko thể nào ít hơn 30 ký tự
                return false;
            }
            var campaigns = Optional.Get(() => _remoteConfig.GetString(KCampaignRemoteKey), string.Empty);
            return campaigns.Contains(curCampaign);
        }

        private static bool IsValidCampaignName(string campaignName) {
            // Tên campaign ko thể nào ít hơn 30 ký tự
            return !string.IsNullOrWhiteSpace(campaignName) && campaignName.Length >= 30;
        }
    }
}