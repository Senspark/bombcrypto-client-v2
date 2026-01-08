using System;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class UnlockTime : MonoBehaviour {
        [FormerlySerializedAs("_text")]
        [SerializeField]
        private Text text;

        private DateTime _value;

        public void Initialize(DateTime value) {
            _value = value;
            Update();
        }

        private void Update() {
            var duration = _value - DateTime.UtcNow;
            text.text = $"{duration.Hours:00}H:{duration.Minutes:00}M:{duration.Seconds:00}S";
            gameObject.SetActive(duration.TotalMilliseconds > 0);
        }
    }
}