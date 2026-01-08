using Game.Dialog.BomberLand.BLGacha;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMRank : MonoBehaviour {
        [SerializeField]
        private Image iconRankFrom;

        [SerializeField]
        private Image iconRankTo;

        [SerializeField]
        private GameObject rankEnd;
        
        [SerializeField]
        private GameObject rankMax;

        [SerializeField]
        private Text rankValueText;

        [SerializeField]
        private BLGachaRes resource;

        public async void SetInfo(PvpRankType rankFrom, int currentPoint, int startPoint, int endPoint) {
            var isMaxRank = rankFrom == PvpRankType.Diamond2;
            var rankTo = isMaxRank ? rankFrom : rankFrom + 1;
            iconRankFrom.sprite = await resource.GetSpriteByPvpRank(rankFrom);
            iconRankTo.sprite = await resource.GetSpriteByPvpRank(rankTo);
            rankEnd.SetActive(!isMaxRank);
            rankMax.SetActive(isMaxRank);
            rankValueText.text = isMaxRank ? $"{currentPoint}" : $"{currentPoint}/{endPoint}";
        }
    }
}