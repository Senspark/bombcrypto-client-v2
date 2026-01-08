using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BlDialogIapPackItem : MonoBehaviour {
        [SerializeField]
        private Image img;

        [SerializeField]
        private TextMeshProUGUI descTxt;

        [SerializeField]
        private GameObject premiumFrame;

        private Action _onClicked;

        public void SetData(Sprite spr, string desc, bool isPremium, Action onClicked) {
            img.sprite = spr;
            descTxt.text = desc;
            _onClicked = onClicked;
            premiumFrame.SetActive(isPremium);
        }
        
        public void OnClicked() {
            _onClicked?.Invoke();
        }
    }
}