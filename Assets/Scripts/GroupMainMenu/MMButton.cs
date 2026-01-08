using System;
using System.Net.Http.Headers;

using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMButton : MonoBehaviour {
        [SerializeField]
        protected Button button;

        [SerializeField]
        private TextMeshProUGUI counterText;

        [SerializeField]
        private Image[] images;

        public Button Button => button;

        private System.Action _onClickedCallback;
        

        public bool Interactable {
            get => button.interactable;
            set {
                InteractableColor(value);
                button.interactable = value;
            }
        }

        private void InteractableColor(bool value) {
            button.interactable = value;
            var color = value ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.9f);
            foreach (var iter in images) {
                iter.color = color;
            }
        }

        public void SetInteractable(bool value) {
            button.interactable = value;
        }
        
        public void SetVisible(bool value) {
            button.gameObject.SetActive(value);
        }

        public void SetCounter(string text) {
            if (string.IsNullOrEmpty(text)) {
                counterText.gameObject.SetActive(false);
                return;
            }
            counterText.gameObject.SetActive(true);
            counterText.text = text;
        }
        
        protected virtual void Awake() {
            button.onClick.AddListener(OnClicked);
        }

        public void SetOnClickedCallback(System.Action callback) {
            _onClickedCallback = callback;
        }

        private void OnClicked() {
            _onClickedCallback?.Invoke();
        }
    }
}