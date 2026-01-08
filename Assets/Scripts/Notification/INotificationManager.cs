using System;

using Senspark;

namespace Notification {
    [Service(nameof(INotificationManager))]
    public interface INotificationManager {
        void RetentionReminder();
        void AddNewBattleAwaitsNotification();
        void AddSeasonCommencedNotification(int seconds);
        void AddSeasonAlreadyNotification(int seconds);
        void AddDailyGiftNotification(int seconds);
        void AddOpenChestNotification(int seconds);
        void CancelSeasonAlreadyNotification();
        void RemoveNotification(int id);
        bool HadAddSeasonNotification { set; get; }
        void SetSeasonEndTime(DateTime endTime);
    }
}