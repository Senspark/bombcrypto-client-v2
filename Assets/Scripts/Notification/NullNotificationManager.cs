
using System;
using System.Threading.Tasks;

using UnityEngine;

namespace Notification {
    public class NullNotificationManager : INotificationManager
    {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
        
        public void RetentionReminder() {
            Debug.Log("Retention Notification");
        }

        public void AddNewBattleAwaitsNotification() {
        }

        public void AddSeasonCommencedNotification(int seconds) {
        }

        public void AddSeasonAlreadyNotification(int seconds) {
        }

        public void AddDailyGiftNotification(int seconds) {
            Debug.Log($"Add Daily Notification with seconds {seconds}");
        }

        public void AddOpenChestNotification(int seconds) {
            Debug.Log($"Add Open Chest Notification with seconds {seconds}");
        }

        public void CancelSeasonAlreadyNotification() {
            Debug.Log($"Remove Notification Season Already");
        }

        public void RemoveNotification(int id) {
        }

        public bool HadAddSeasonNotification { get; set; }

        public void SetSeasonEndTime(DateTime endTime) { }
    }
}
