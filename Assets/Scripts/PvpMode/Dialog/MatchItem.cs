using PvpMode.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.Dialogs {
    public class MatchItem : MonoBehaviour {
        [SerializeField]
        private Text matchIdText;

        [SerializeField]
        private Text opponentText;

        [SerializeField]
        private Text timeText;

        [SerializeField]
        private Text dateText;

        [SerializeField]
        private Text resultText;

        [SerializeField]
        private Toast toast;
        
        public void SetInfo(string matchId, string opponentName, string opponent, string time, string date, bool isWin) {
            matchIdText.text = $"{matchId}";
            opponentText.text = $"{opponentName ?? opponent}";
            timeText.text = $"{time}";
            dateText.text = $"{date}";
            var strResult = isWin ? "WIN" : "LOSE";
            resultText.text = $"{strResult}";
        }

        public void OnMatchIdClicked(Text text) {
            GUIUtility.systemCopyBuffer = text.text;
            ShowToast();
        }

        private void ShowToast() {
            toast.Show();
        }
    }
}