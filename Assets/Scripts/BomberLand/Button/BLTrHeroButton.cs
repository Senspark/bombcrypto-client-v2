using System;

using Game.Dialog;

using UnityEngine;

namespace BomberLand.Button {
    public class BLTrHeroButton : MonoBehaviour {
        [SerializeField]
        private TrHeroAnimAvatar avatar;

        private int _index;
        private Action<int> _onClickCallback;

        public UIHeroData Hero { get; private set; }

        public void Initialize(int index, UIHeroData hero, Action<int> callback) {
            Hero = hero;
            _index = index;
            _onClickCallback = callback;
            avatar.UpdateHero(hero);
        }

        public void SetSelected(bool value) {
            avatar.ShowHighLight(value);
        }

        public void OnClicked() {
            _onClickCallback?.Invoke(_index);
        }
    }
}