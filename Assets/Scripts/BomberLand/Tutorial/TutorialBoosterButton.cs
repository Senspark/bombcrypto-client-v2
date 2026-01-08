using System;

using PvpMode.Manager;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Tutorial {
    public class TutorialBoosterButton : MonoBehaviour {
        [SerializeField]
        private GameObject highlight;

        [SerializeField]
        private BoosterType boosterType;

        [SerializeField]
        private Text quality;

        private bool _isChoose;

        public BoosterType BoosterType => boosterType;

        public void OnClicked() {
            _isChoose = !_isChoose;
            highlight.SetActive(_isChoose);
        }

        public void SetHighlight(bool isActive) {
            highlight.SetActive(isActive);
        }

        public void SetQuality(int num) {
            quality.text = $"{num}";
        }
    }
}