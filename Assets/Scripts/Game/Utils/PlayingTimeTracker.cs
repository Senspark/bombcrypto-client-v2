using System;
using System.Collections.Generic;

using Analytics;

using Senspark;

using Engine.Components;
using Engine.Manager;

using UnityEngine;

namespace Utils {
    public class PlayingTimeTracker : MonoBehaviour {
        private bool _isTracking;
        private ILevelManager _levelManager;
        private IPlayerManager _playerManager;
        private float _playingTime;
        private List<BotManager> _botManagers = new List<BotManager>();
        private IAnalytics _analytics;

        private void Awake() {
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        }

        private void OnDisable() {
            StopAndSendTracking();
        }

        private void OnApplicationQuit() {
            StopAndSendTracking();
        }

        public void Init(ILevelManager levelManager, IPlayerManager playerManager) {
            _levelManager = levelManager;
            _playerManager = playerManager;
            _playingTime = 0;
        }

        public void BeginTrack() {
            _botManagers.Clear();
            foreach (var player in _playerManager.Players) {
                if (!player) {
                    return;
                }
                var bot = player.GetComponent<BotManager>();
                if (!_botManagers.Exists(e => e == bot)) {
                    _botManagers.Add(bot);
                }
            }

            var allSleep = DetectIfAllPlayerIsSleeping();
            var willStopTracking = _isTracking && allSleep;
            if (willStopTracking) {
                StopAndSendTracking();
            }
            _isTracking = !allSleep;
        }

        public void Step(float delta) {
            if (!_isTracking) {
                return;
            }

            _playingTime += delta;
            
            if (_levelManager.IsCompleted) {
                StopAndSendTracking();
                return;
            }

            if(DetectIfAllPlayerIsSleeping()) {
                StopAndSendTracking();
            }
        }

        public void StopAndSendTracking() {
            if (_playingTime <= 0) {
                return;
            }

            _analytics.TreasureHunt_TrackPlayingTime(_playingTime);
            _isTracking = false;
            _playingTime = 0;
        }
        
        private bool DetectIfAllPlayerIsSleeping() {
            for (var i = 0; i < _botManagers.Count; i++) {
                if (!_botManagers[i].IsSleeping) {
                    return false;
                }
            }

            return true;
        }
    }
}