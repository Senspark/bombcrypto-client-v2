using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

namespace Senspark.Notifications {
    public abstract class AnalyticsUnityMobileNotification :
        ObserverManager<NotificationObserver>,
        IUnityMobileNotification {
        protected abstract IAnalyticsManager AnalyticsManager { get; set; }
        protected abstract IUnityMobileNotification Bridge { get; set; }
        private ObserverHandle _handle;

        private const string KNotificationClicked = "track_notification_clicked";
        private const string KNotificationShown = "track_notification_shown";

        public virtual UniTask Initialize() {
            _handle = new ObserverHandle();
            _handle.AddObserver(Bridge, new NotificationObserver {
                OnNotificationShown = OnNotificationShown,
                OnNotificationClicked = OnNotificationClicked
            });
            return UniTask.CompletedTask;
        }

        public virtual void Dispose() {
            _handle?.Dispose();
        }

        public bool GetDataFromClickedNotification() {
            // var isClicked = Bridge.GetDataFromClickedNotification(out id, out var mergedValue);
            // if (!isClicked || !TryParseMergedValue(id, mergedValue, out var mergedData)) {
            //     extraData = null;
            //     return false;
            // }
            // TrackAnalytics(id, mergedData.TrackingData, KNotificationClicked);
            // extraData = mergedData.OriginalData;
            return true;
        }

        public void RepeatFromNow(int id, bool useBanner, string title, string message, string extraData,
            int delaySeconds, int repeatSeconds) {
            var mergedData = CreateMergedData(id, useBanner, message, extraData);
            var d = new ScheduleDataTime(id, title, message).AddDelay(delaySeconds).AddRepeat(repeatSeconds)
                .AddExtraData(mergedData);
            Bridge.Schedule(d);
        }

        public void Schedule(ScheduleDataTime data) {
            var mergedData = CreateMergedData(data.Id, data.UseBackground, data.Message, data.ExtraData);
            data.ExtraData = mergedData;
            Bridge.Schedule(data);
        }

        public void RepeatAtExactlyTime(int id, bool useBanner, string title, string message, string extraData,
            int atHour, int atMinute, int repeatAfterDays) {
            var mergedData = CreateMergedData(id, useBanner, message, extraData);
            var d = new ScheduleDataDay(id, title, message).AddTime(atHour, atMinute).AddRepeat(repeatAfterDays)
                .AddExtraData(mergedData);
            Bridge.Schedule(d);
        }

        public void Schedule(ScheduleDataDay data) {
            var mergedData = CreateMergedData(data.Id, data.UseBackground, data.Message, data.ExtraData);
            data.ExtraData = mergedData;
            Bridge.Schedule(data);
        }

        public void CancelNotification(List<int> ids) {
            Bridge.CancelNotification(ids);
        }

        public bool CanShowNotification() {
            return Bridge.CanShowNotification();
        }

        public UniTask<bool> AskPermission() {
            return Bridge.AskPermission();
        }

        public UniTask<NotificationCallbackData> GetNotificationData() {
            return Bridge.GetNotificationData();
        }

        private static string CreateMergedData(int id, bool useBanner, string message, string extraData) {
            extraData ??= string.Empty;
            var type = useBanner ? 1 : 0;
            var now = TimeUtils.ConvertDateTimeToEpochMilliSeconds(DateTime.UtcNow);
            var trackingData =
                JsonConvert.SerializeObject(new NotificationTracking(type, id, now, message), Formatting.None);
            var mergedData = JsonConvert.SerializeObject(new MergedData(extraData, trackingData), Formatting.None);
            return mergedData;
        }

        private void OnNotificationShown(int id, string mergedValue) {
            if (!TryParseMergedValue(id, mergedValue, out var mergedData)) {
                return;
            }
            DispatchEvent(e => e.OnNotificationShown?.Invoke(id, mergedData.OriginalData));
            TrackAnalytics(id, mergedData.TrackingData, KNotificationShown);
        }

        private void OnNotificationClicked(int id, string mergedValue) {
            if (!TryParseMergedValue(id, mergedValue, out var mergedData)) {
                return;
            }
            DispatchEvent(e => e.OnNotificationShown?.Invoke(id, mergedData.OriginalData));
            TrackAnalytics(id, mergedData.TrackingData, KNotificationClicked);
        }

        private void TrackAnalytics(int id, string trackingValue, string evName) {
            if (AnalyticsManager == null || string.IsNullOrWhiteSpace(trackingValue)) {
                return;
            }
            try {
                FastLog.Info($"[Senspark] Received clicked from Notification id: {id}");
                var trackingData = JsonConvert.DeserializeObject<NotificationTracking>(trackingValue);
                trackingData.Name = evName;
                AnalyticsManager.LogEvent(trackingData);
            } catch (Exception e) {
                FastLog.Error(e.Message);
            }
        }

        private static bool TryParseMergedValue(int id, string mergedValue, out MergedData data) {
            if (id < 0 || string.IsNullOrWhiteSpace(mergedValue)) {
                data = null;
                return false;
            }
            try {
                data = JsonConvert.DeserializeObject<MergedData>(mergedValue);
                return data != null;
            } catch (Exception e) {
                FastLog.Error(e.Message);
            }
            data = null;
            return false;
        }

        [Serializable] // 
        private class MergedData {
            [JsonProperty("originalData")]
            public readonly string OriginalData;

            [JsonProperty("trackingData")]
            public readonly string TrackingData;

            [JsonConstructor] // 
            public MergedData(string originalData, string trackingData) {
                OriginalData = originalData;
                TrackingData = trackingData;
            }
        }
    }
}