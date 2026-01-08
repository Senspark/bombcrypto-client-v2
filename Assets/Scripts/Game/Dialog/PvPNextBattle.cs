using System;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class PvPNextBattle : MonoBehaviour {
        [FormerlySerializedAs("_time")]
        [SerializeField]
        private Text time;

        [FormerlySerializedAs("_users")]
        [SerializeField]
        private Text users;

        public void UpdateUI(TimeSpan time, string[] users) {
            this.time.text = $"{time.Hours}:{time.Minutes}:{time.Seconds}";
            this.users.text = users == null
                ? string.Empty
                : string.Join("\n", users.Select(it => $"{it[..4]}...{it[^4..]}"));
        }
    }
}