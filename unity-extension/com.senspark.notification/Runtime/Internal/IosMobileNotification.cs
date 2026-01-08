#if UNITY_IOS
using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Unity.Notifications.iOS;

namespace Senspark.Notifications {
    /// <summary>
    /// https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.1/manual/iOS.html
    /// </summary>
    public class IosMobileNotification : ObserverManager<NotificationObserver>, IUnityMobileNotification {
        private const string DefaultId = "_notification_";
        private const string DefaultCategory = "category_1";
        private const string DefaultThreadId = "thread1";

        public async UniTask Initialize() {
            const AuthorizationOption authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using var req = new AuthorizationRequest(authorizationOption, true);
            await UniTask.WaitUntil(() => req.IsFinished);
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        public void Dispose() { }

        public bool GetDataFromClickedNotification(out int id, out string extraData) {
            // FIXME: not implemented
            id = -1;
            extraData = null;
            return false;
        }

        public void RepeatFromNow(int id, bool useBanner, string title, string message, string extraData,
            int delaySeconds, int repeatSeconds) {
            var timeTrigger = new iOSNotificationTimeIntervalTrigger {
                TimeInterval = TimeSpan.FromSeconds(repeatSeconds),
                Repeats = true
            };

            SetNotification(id, title, message, timeTrigger);
        }

        public void Schedule(ScheduleDataTime data) {
            
            //// timeTrigger chỉ có TimeInterval và Repeat => bắt dầu fire sau TimeInterval và lặp lại sau TimeInterval.
            //// Không có Time Delay và Time Repeat riêng biệt
            // RepeatFromNow(data.Id, data.UseBackground, data.Title, data.Message, data.ExtraData, data.DelaySeconds,
            //     data.RepeatSeconds);
            if (data.RepeatSeconds > 0) {
                RepeatFromNow(data.Id, data.UseBackground, data.Title, data.Message, data.ExtraData, data.DelaySeconds,
                    data.RepeatSeconds);
            }
            
            var id = data.Id;
            var title = data.Title;
            var message = data.Message;
            var delaySeconds = data.DelaySeconds;
            var timeTrigger = new iOSNotificationTimeIntervalTrigger {
                TimeInterval = TimeSpan.FromSeconds(delaySeconds),
                Repeats = false
            };
            
            SetNotification(id, title, message, timeTrigger);
        }

        public void RepeatAtExactlyTime(int id, bool useBanner, string title, string message, string extraData,
            int atHour, int atMinute, int repeatAfterDays) {
            var calendarTrigger = new iOSNotificationCalendarTrigger {
                Hour = atHour,
                Minute = atMinute,
                Second = 0,
                Repeats = true
            };

            SetNotification(id, title, message, calendarTrigger);
        }

        public void Schedule(ScheduleDataDay data) {
            RepeatAtExactlyTime(data.Id, data.UseBackground, data.Title, data.Message, data.ExtraData, data.AtHour,
                data.AtMinute, data.RepeatAfterDays);
        }

        public void CancelNotification(List<int> ids) {
            foreach (var id in ids) {
                CancelNotification(id);
            }
        }

        public bool CanShowNotification() {
            return true;
        }

        public UniTask<bool> AskPermission() {
            return UniTask.FromResult(true);
        }

        public UniTask<NotificationCallbackData> GetNotificationData() {
            return UniTask.FromResult<NotificationCallbackData>(null);
        }

        public void CancelNotification(int id) {
            var identifier = $"{DefaultId}{id}";
            iOSNotificationCenter.RemoveScheduledNotification(identifier);
        }

        private static void SetNotification(int id, string title, string message, iOSNotificationTrigger trigger) {
            var notification = new iOSNotification {
                // You can specify a custom identifier which can be used to manage the notification later.
                // If you don't provide one, a unique string will be generated automatically.
                Identifier = $"{DefaultId}{id}",
                Title = title,
                Body = message,
                Subtitle = message,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = DefaultCategory,
                ThreadIdentifier = DefaultThreadId,
                Trigger = trigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
        }
    }
}
#endif