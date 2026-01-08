using System;
using System.Threading;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Notification;

using Senspark;

using UnityEngine;

namespace Utils {
    public class ProfileCardInitializer : MonoBehaviour {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private BLProfileCard profileCard;

        private IServerManager _serverManager;
        private INotificationManager _notificationManager;
        
        private CancellationTokenSource _cts;
        private bool _initialized;

        private void Awake() {
            Initialized().Forget();
        }

        public UniTask Initialized() {
            if (_initialized) {
                return UniTask.CompletedTask;
            }
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _notificationManager = ServiceLocator.Instance.Resolve<INotificationManager>();

            _initialized = true;
            return UniTask.CompletedTask;
        }

        
        public void TryLoadData() {
            if (_cts != null) {
                return;
            }
            _cts = new CancellationTokenSource();
            UniTask.Void(async (c) => {
                ReloadProfile();
                var rankingResult = await _serverManager.Pvp.GetPvpRanking();
                if (!c.IsCancellationRequested) {
                    AddSeasonNotification(rankingResult.RemainTime);
                }
            }, _cts.Token);
        }

        private void ReloadProfile() {
            profileCard.InitializeAsync(canvas).Forget();
        }

        private void AddSeasonNotification(int minutes) {
            if (_notificationManager.HadAddSeasonNotification) {
                return;
            }
            _notificationManager.HadAddSeasonNotification = true;
            
            //season start time = 0 - minutes
            if (minutes >= 0) {
                return;
            }
            // ghi nhận end Time để nhận biết đã vào mùa  hay chưa ?
            _notificationManager.SetSeasonEndTime(DateTime.Now.Add(TimeSpan.FromMinutes(-minutes)));

            var durationStart = TimeSpan.FromMinutes(-minutes).TotalSeconds;
            var durationPrompt = durationStart + TimeSpan.FromHours(2).TotalSeconds;
            _notificationManager.AddSeasonCommencedNotification((int)durationStart);
            _notificationManager.AddSeasonAlreadyNotification((int)durationPrompt);
        }
        
        private void OnDestroy() {
            if (_cts == null) {
                return;
            }
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}