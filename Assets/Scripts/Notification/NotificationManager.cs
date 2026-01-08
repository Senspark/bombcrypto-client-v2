using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Senspark;
using Senspark.Notifications;

namespace Notification {
    public class NotificationManager : INotificationManager
    {
        private IUnityMobileNotification _notificationService;
        private Task<bool> _initializationTask;

        public NotificationManager(IAnalyticsManager analytics) {
            _notificationService = new UnityMobileNotificationBuilder.Builder {
                AnalyticsManager = analytics,
                EnableLog = true
            }.Build();        
        }
        
        public Task<bool> Initialize() => _initializationTask ??= InitializeImpl();
        private async Task<bool> InitializeImpl()
        {
            await _notificationService.Initialize();
            RetentionReminder();
            AddNewBattleAwaitsNotification();
            return true;
        }

        private const int RetentionID = 1;
        private const int BattleAwaitsID = 5;
        private const int SeasonCommencedID = 6;
        private const int SeasonAlreadyId = 7;
        private const int OpenChestId = 8;
        private const int DailyID = 9;

        private DateTime _seasonEndTime;
        public bool HadAddSeasonNotification { set; get; } = false;

        private void SendNotification(int id, string title, string body, int background,
            int delaySeconds) {

            // IOS: Time interval must be greater than 0.
            if (delaySeconds <= 0) {
                return;
            }
            var schedule = new ScheduleDataTime(id, title, body)
                .AddDelay(delaySeconds)
                .AddBackground(background);
            _notificationService.Schedule(schedule);
        }

        private void SendNotificationAtTime(int id, string title, string body, int background,
            int hour, int minute) {
            var schedule = new ScheduleDataDay(id, title, body)
                .AddTime(hour, minute)
                .AddRepeat(1)
                .AddBackground(background);
            
            _notificationService.Schedule(schedule);
        }
        
        public void RetentionReminder() {
            var title = "Your heroes miss you";
            var body = "Get back in the action and save the kingdom 💣";
            var delay = (int) TimeSpan.FromHours(48).TotalSeconds;
            SendNotification(RetentionID, title, body, 1, delay);
        }

        public void AddNewBattleAwaitsNotification() {
            var title = "New battles await";
            var body = "💥 New day, new opponents to blow up 💥";
            SendNotificationAtTime(BattleAwaitsID, title, body, 0, 20, 0);
        }

        public void AddSeasonCommencedNotification(int seconds) {
            var title = "New season has begun!";
            var body = "💣 Ready your heroes and conquer the leaderboard 💣";
            SendNotification(SeasonCommencedID, title, body, 1, seconds);
        }

        public void AddSeasonAlreadyNotification(int seconds) {
            var title = "Season already started";
            var body = "📅 Friendly reminder for you to start the season and earn big rewards";
            SendNotification(SeasonAlreadyId, title, body, 1, seconds);
        }

        public void AddOpenChestNotification(int seconds) {
            var title = "Open Chest now!";
            var body = "See what's hidden inside your newest Chest 🗝️";
            SendNotification(OpenChestId, title, body, 2, seconds);
        }
        
        public void AddDailyGiftNotification(int seconds) {
            var title = "Daily rewards are ready!";
            var body = "🎁 Daily gifts have been opened, receive gifts now";
            SendNotification(DailyID, title, body, 2, seconds);
        }

        public void CancelSeasonAlreadyNotification() {
            // chỉ cancel khi season đã vào mùa
            if (_seasonEndTime <= DateTime.Now) { 
                return;
            }
            RemoveNotification(SeasonAlreadyId);
        }
        
        public void RemoveNotification(List<int> id) {
            _notificationService.CancelNotification(id);
        }
        
        public void RemoveNotification(int id) {
            _notificationService.CancelNotification(new List<int>(id));
        }

        public void SetSeasonEndTime(DateTime endTime) {
            _seasonEndTime = endTime;
        }
    }
}
