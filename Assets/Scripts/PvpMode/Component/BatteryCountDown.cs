using System;

using App;

using Engine.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Component {
    public class BatteryCountDown : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Text timerText;

        private const int MaxBattery = 3;

        private PlayerData _player;
        private float _process;
        private long _timeFillBattery;
        private bool _stopProcess;

        public System.Action<int> OnUpdateBattery;
        
        private void Update() {
            if (_stopProcess) {
                return;
            }

            _process += Time.deltaTime;
            if (_process < 2f) {
                return;
            }

            _process = 0;
            _stopProcess = !UpdateTimerText();
        }

        public void SetTimeFillBattery(PlayerData player) {
            _player = player;
            _timeFillBattery = player.timeRefillBattery;
            _stopProcess = false;
            _process = 1;
        }

        private bool UpdateTimerText() {
            _timeFillBattery -= 1;

            if (_timeFillBattery <= 0) {
                var battery = _player.battery;
                battery = IncreaseBattery(battery);
                if (battery == MaxBattery) {
                    button.interactable = true;
                    gameObject.SetActive(false);
                } else {
                    _timeFillBattery = 10800; // (3 * 60 * 60) = 3 hours
                    _player.timeRefillBattery = _timeFillBattery;
                }
                _player.battery = battery;
                OnUpdateBattery?.Invoke(battery);
            }

            var timeString = Epoch.GetTimeStringOneHour((int) _timeFillBattery);
            timerText.text = timeString;
            return true;
        }

        private static int IncreaseBattery(int battery) {
            battery += 1;
            if (battery > MaxBattery) {
                battery = MaxBattery;
            }
            return battery;
        }
    }
}