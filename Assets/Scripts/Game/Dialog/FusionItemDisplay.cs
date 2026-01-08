using System;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class FusionItemDisplay : MonoBehaviour {
        [SerializeField]
        private GameObject frameChoose;

        [SerializeField]
        private Avatar avatar;
        
        [SerializeField]
        private Text charName;

        [SerializeField]
        private Text levelText;

        [SerializeField]
        private HeroRarityDisplay rarityDisplay;

        public PlayerData PlayerData { get; private set; }
        public int ItemIndex { get; private set; }
        
        private ISoundManager _soundManager;
        private Action<int> _onClicked;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void Init(int itemIndex, Action<int> onClicked) {
            ItemIndex = itemIndex;
            _onClicked = onClicked;
        }

        public void SetData(PlayerData playerData) {
            PlayerData = playerData;
            
            if (playerData == null) {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            avatar.ChangeImage(playerData);
            charName.text = playerData.heroId.Id.ToString();
            rarityDisplay.Show(playerData.rare);
            levelText.text = $"LV{playerData.level}";
            SetChoose(false);
        }

        public void OnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onClicked?.Invoke(ItemIndex);
        }
        
        public void SetChoose(bool value) {
            frameChoose.SetActive(value);
        }
    }
}