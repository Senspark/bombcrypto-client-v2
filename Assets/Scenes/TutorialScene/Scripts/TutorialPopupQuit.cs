using System;

using UnityEngine;

namespace BomberLand.Tutorial {
    public class TutorialPopupQuit : MonoBehaviour {
        private Action _onCancelCallback;

        private Action _onConfirmCallback;

        public void SetOnConfirmCallback(Action onConfirmCallback) {
            _onConfirmCallback = onConfirmCallback;
        }

        public void OnCancelButtonClicked() {
            _onCancelCallback?.Invoke();
            Hide();
        }

        public void OnConfirmButtonClicked() {
            _onConfirmCallback?.Invoke();
        }
        
        public void Show() {
            gameObject.SetActive(true);
        }

        public void Hide() {
            gameObject.SetActive(false);
        }
    }
}