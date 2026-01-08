using System;

using App;

using Game.UI;

using UnityEngine;

namespace GroupMainMenu {
    public class MMHeaderBar : MonoBehaviour {
        private Action<BlockRewardType> _onTokenClickedCallback;

        private ChestRewardTokenDisplayUI[] _tokens;

        private void Awake() {
            _tokens = GetComponentsInChildren<ChestRewardTokenDisplayUI>();
        }

        public void SetOnTokenClickedCallback(Action<BlockRewardType> callback) {
            _onTokenClickedCallback = callback;
            foreach (var token in _tokens) {
                token.SetOnClickedCallback(_onTokenClickedCallback);
            }
        }

        public void SetInteractable(bool value) {
            foreach (var token in _tokens) {
                token.Interactable = value;
            }
        }
    }
}