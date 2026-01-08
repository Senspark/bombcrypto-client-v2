using PvpMode.Services;

using TMPro;

using UnityEngine;

namespace PvpMode.UI
{
    public class PvpRanking : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI rankNumberText;
        
        [SerializeField]
        private TextMeshProUGUI winText;

        [SerializeField]
        private TextMeshProUGUI loseText;

        [SerializeField]
        private TextMeshProUGUI rateText;

        [SerializeField]
        private TextMeshProUGUI rankText;

        [SerializeField]
        private GameObject[] medals;
        
        public void SetCurrentInfo(IPvpRankingItemResult item) {
            winText.text = $"{item.WinMatch}";
            loseText.text = $"{item.TotalMatch - item.WinMatch}";
            var percent = 0f;
            if (item.TotalMatch > 0) {
                percent = item.WinMatch * 100f / item.TotalMatch;
            }
            rateText.text = $"{percent:0.00}%";
            rankText.text = $"{item.Point}";

            if (rankNumberText != null){
                rankNumberText.text = $"{item.RankNumber}";
                if (medals is {Length: >= 3}) {
                    for (var i = 0; i < 3; i++) {
                        medals[i].SetActive(i == item.RankNumber - 1);
                    }    
                }
            }
        }
    }
}