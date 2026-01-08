#if UNITY_ANDROID
using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Senspark.Notifications {
    /// <summary>
    /// https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.1/manual/Android.html
    /// </summary>
    public class AndroidMobileNotification : ObserverManager<NotificationObserver>, IUnityMobileNotification {

        private readonly ICustomNotificationBridge _bridge;

        public AndroidMobileNotification(
            Color iconColor,
            string smallIcon,
            bool enableLog
        ) {
            _bridge = new AndroidCustomNotificationBridge(enableLog);
        }

        public UniTask Initialize() {
            return UniTask.CompletedTask;
        }

        public void Dispose() {
        }

        public void RepeatFromNow(
            int id,
            bool useBanner,
            string title,
            string message,
            string extraData,
            int delaySeconds,
            int repeatSeconds
        ) {
            var data = new ScheduleDataTime(id, title, message)
                .AddDelay(delaySeconds)
                .AddRepeat(repeatSeconds)
                .AddExtraData(extraData);
            if (useBanner) {
                data.AddBackground(0);
            }
            Schedule(data);
        }

        public void Schedule(ScheduleDataTime data) {
            data.Title ??= Application.productName;
            _bridge.Schedule(data);
        }

        public void RepeatAtExactlyTime(
            int id,
            bool useBanner,
            string title,
            string message,
            string extraData,
            int atHour,
            int atMinute,
            int repeatAfterDays
        ) {
            var data = new ScheduleDataDay(id, title, message)
                .AddTime(atMinute, atMinute)
                .AddRepeat(repeatAfterDays)
                .AddExtraData(extraData);
            if (useBanner) {
                data.AddBackground(0);
            }
            Schedule(data);
        }

        public void Schedule(ScheduleDataDay data) {
            data.Title ??= Application.productName;
            _bridge.Schedule(data);
        }

        public void CancelNotification(List<int> ids) {
            _bridge.CancelNotification(ids);
        }

        public bool CanShowNotification() {
            return _bridge.CanShowNotification();
        }

        public UniTask<bool> AskPermission() {
            return _bridge.AskPermission();
        }

        public UniTask<NotificationCallbackData> GetNotificationData() {
            return _bridge.GetNotificationData();
        }
    }
}
#endif