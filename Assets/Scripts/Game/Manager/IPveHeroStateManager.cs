using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using Engine.Utils;

using UnityEngine;

namespace App {
    [Service(nameof(IPveHeroStateManager))]
    public interface IPveHeroStateManager : IService {
        void ChangeHeroState(HeroId heroId, HeroStage state);
        void ChangeHeroState(HeroId[] heroId, HeroStage state);
        void Update(float dt);
    }

    public class DefaultPveHeroStateManager : IPveHeroStateManager {
        private readonly IServerManager _serverManager;
        private readonly ILogManager _logManager;
        private readonly Dictionary<HeroId, ExpectedState> _expectedStates;
        private readonly ObserverHandle _handle;
        private readonly Timer _timer;

        // Mong muốn server phải response trong khoảng thời gian này
        private const float MAXWaitingSeconds = 60;

        public DefaultPveHeroStateManager(ILogManager logManager, IServerManager serverManager) {
            _expectedStates = new Dictionary<HeroId, ExpectedState>();
            _serverManager = serverManager;
            _logManager = logManager;

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnHeroChangeState = OnHeroChangeState
            });

            _timer = new Timer(MAXWaitingSeconds, ResendMessages, true);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
            _handle.Dispose();
        }
        
        public void Update(float dt) {
            _timer.Update(dt);
        }

        public void ChangeHeroState(HeroId heroId, HeroStage state) {
            if (!CanSendMessage(heroId, state)) {
                return;
            }

            SendMessage(heroId, state);
        }

        public async void ChangeHeroState(HeroId[] heroId, HeroStage state) {
            // Chỉ cần 1 hero được gửi message là sẽ cho phép gửi lại 1 list
            var sum = 0;
            foreach (var i in heroId) {
                var canSend = CanSendMessage(i, state);
                if (canSend) {
                    sum++;
                    AddRecord(i, state);
                }
            }
            if (sum > 0) {
                if (AppConfig.IsSolana()) {
                    await _serverManager.UserSolanaManager.ChangeBomberManStageSol(heroId, state);
                } else {
                    await _serverManager.Pve.ChangeBomberManStage(heroId, state);
                }
            }
        }
        private bool CanSendMessage(HeroId heroId, HeroStage state) {
            if (!_expectedStates.ContainsKey(heroId)) {
                return true;
            }

            // Đã từng gửi message nhưng chưa có response
            var expectedState = _expectedStates[heroId];

            // Nếu gửi state khác thì ok
            if (expectedState.Stage != state) {
                return true;
            }

            // Chỉ cho gửi nếu đã quá thời gian quy định
            return expectedState.LastMessageTimestamp.AddSeconds(MAXWaitingSeconds) <= DateTime.Now;
        }

        private void SendMessage(HeroId heroId, HeroStage state) {
            AddRecord(heroId, state);
            switch (state) {
                case HeroStage.Home:
                    if (AppConfig.IsSolana()) {
                        _serverManager.UserSolanaManager.GoHomeSol(heroId);
                    } 
                    else {
                        _serverManager.Pve.GoHome(heroId);
                    }
                    break;
                case HeroStage.Sleep:
                    if (AppConfig.IsSolana()) {
                        _serverManager.UserSolanaManager.GoSleepSol(heroId);
                    } 
                    else {
                        _serverManager.Pve.GoSleep(heroId);
                    }
                    break;
                case HeroStage.Working:
                    if (AppConfig.IsSolana()) {
                        _serverManager.UserSolanaManager.GoWorkSol(heroId);
                    } 
                    else {
                        _serverManager.Pve.GoWork(heroId);
                    }
                    break;
                default:
                    throw new Exception($"Invalid request state {state} for hero id {heroId}");
            }
        }

        private void AddRecord(HeroId heroId, HeroStage state) {
            _expectedStates[heroId] = new ExpectedState {
                Stage = state,
                LastMessageTimestamp = DateTime.Now
            };
        }

        private void OnHeroChangeState(IPveHeroDangerous data) {
            _expectedStates.Remove(data.HeroId);
        }

        private void ResendMessages() {
            var now = DateTime.Now;
            var expiredMessages =
                _expectedStates.Where(e => e.Value.LastMessageTimestamp.AddSeconds(MAXWaitingSeconds) < now).ToList();
            foreach (var (id, expectedState) in expiredMessages) {
                _logManager.Log($"Resend hero: {id} state: {expectedState.Stage}");
                SendMessage(id, expectedState.Stage);
            }
        }

        private class ExpectedState {
            public HeroStage Stage;
            public DateTime LastMessageTimestamp;
        }
    }
}