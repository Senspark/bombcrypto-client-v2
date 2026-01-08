using System;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Game.Dialog {
    public class PvPBattle : MonoBehaviour {
        [FormerlySerializedAs("_battleIdText")]
        [SerializeField]
        private Text battleIdText;

        [FormerlySerializedAs("_seasonIdText")]
        [SerializeField]
        private Text seasonIdText;

        [FormerlySerializedAs("_userText")]
        [SerializeField]
        private Text userText;

        public void UpdateUI(string battleId, int seasonId, string[] users) {
            battleIdText.alignment = TextAnchor.MiddleLeft;
            battleIdText.text = battleId;
            seasonIdText.alignment = TextAnchor.MiddleRight;
            seasonIdText.text = $"{seasonId}";
            userText.text = string.Join("\n",
                users.Select(it => $"{it[..4]}...{it[^4..]}"));
        }
    }
}