using System;
using System.Xml.Serialization;

using App;

using Engine.Utils;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace StoryMode.Component {
    public class LockCountdown : MonoBehaviour {
        [SerializeField]
        private Button button;
        
        [SerializeField]
        private Text timerText;
        
        private ILanguageManager _languageManager;
        private long _remainSeconds;
        private float _process;
        private long _timeUnlock;
        private bool _stopProcess;    
    
        private void Awake() {
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        }

        private void Update() {
            if (_stopProcess) {
                return;
            }

            _process += Time.deltaTime;
            if (_process < 1) {
                return;
            }

            _process = 0;
            _stopProcess = !UpdateTimerText();
        }

        public void SetTimeUnlock(long timeUnlock) {
            _timeUnlock = timeUnlock;
            _stopProcess = false;
            _process = 1;
        }

        private bool UpdateTimerText() {
            var timestamp = (DateTime.Now.Ticks / TimeSpan.TicksPerSecond);
            _remainSeconds = _timeUnlock - timestamp;

            if (_remainSeconds <= 0) {
                button.interactable = true;
                gameObject.SetActive(false);
                return false;
            }

            var timeString = Epoch.GetTimeString((int) _remainSeconds);
            var lockTime = _languageManager.GetValue("ui_lock_time");
            timerText.text = lockTime + " " + timeString;
            return true;
        }
    }
}
