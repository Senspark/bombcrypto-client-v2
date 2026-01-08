using System;

using Game.UI.Information;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class DialogInformationTab : MonoBehaviour {
        [SerializeField]
        private Text tabNameTxt;

        [SerializeField]
        private Image image;

        public InformationData Data { get; private set; }
        
        private Action<DialogInformationTab> _onClicked;

        public void Init(InformationData data, Action<DialogInformationTab> onClicked) {
            Data = data;
            tabNameTxt.text = data.displayName;
            _onClicked = onClicked;
        }

        public void SetSize(Vector2 sizeDelta, Sprite spr) {
            image.rectTransform.sizeDelta = sizeDelta;
            image.sprite = spr;
        }

        public void OnTabClicked() {
            _onClicked?.Invoke(this);
        }
    }
}