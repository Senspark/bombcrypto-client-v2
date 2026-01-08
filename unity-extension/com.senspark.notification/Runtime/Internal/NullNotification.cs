using System.Collections.Generic;

using Cysharp.Threading.Tasks;

namespace Senspark.Notifications {
    public class NullNotification : ObserverManager<NotificationObserver>, IUnityMobileNotification {
        public UniTask Initialize() {
            return UniTask.CompletedTask;
        }

        public void Dispose() {
        }

        public bool GetDataFromClickedNotification(out int id, out string extraData) {
            id = -1;
            extraData = string.Empty;
            return false;
        }

        public void RepeatFromNow(int id, bool useBanner, string title, string message, string extraData,
            int delaySeconds, int repeatSeconds) {
            FastLog.Info(
                $"[Senspark] Schedule Notification {message} at {delaySeconds} seconds from now, repeat every {repeatSeconds} seconds");
        }

        public void Schedule(ScheduleDataTime data) {
            FastLog.Info(
                $"[Senspark] Schedule Notification {data.Message} at {data.DelaySeconds} seconds from now, repeat every {data.RepeatSeconds} seconds");
        }

        public void RepeatAtExactlyTime(int id, bool useBanner, string title, string message, string extraData,
            int atHour, int atMinute, int repeatAfterDays) {
            FastLog.Info(
                $"[Senspark] Schedule Notification {message} at {atHour}h{atMinute}m, repeat after {repeatAfterDays} days");
        }

        public void Schedule(ScheduleDataDay data) {
            FastLog.Info(
                $"[Senspark] Schedule Notification {data.Message} at {data.AtHour}h{data.AtMinute}m, repeat after {data.RepeatAfterDays} days");
        }

        public void CancelNotification(List<int> ids) {
            FastLog.Info($"[Senspark] Cancel Notification ids {string.Join(',', ids)}");
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
            FastLog.Info($"[Senspark] Cancel Notification id {id}");
        }
    }
}