using System;

using App;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class TermServiceConfirm : MonoBehaviour {
        [SerializeField]
        private Sprite rectOn;

        [SerializeField]
        private Sprite rectOff;

        [SerializeField]
        private Image imageButtonAccept;

        [SerializeField]
        private Button buttonAccept;

        private bool _chooseAccept;
        private Action _onAccepted;

        private void Awake() {
            if(buttonAccept) {
                buttonAccept.interactable = false;
            }
        }

        public void Init(Action onAccepted) {
            _onAccepted = onAccepted;
        }

        public void OnAcceptBtnClicked() {
            _onAccepted?.Invoke();
        }

        public void OnToggleClicked() {
            _chooseAccept = !_chooseAccept;
            if (_chooseAccept) {
                imageButtonAccept.sprite = rectOn;
                buttonAccept.interactable = true;
            } else {
                imageButtonAccept.sprite = rectOff;
                buttonAccept.interactable = false;
            }
        }

        public void OnTermAndServiceClicked() {
            WebGLUtils.OpenUrl("https://senspark.com/term-of-service", true);
        }
    }
}