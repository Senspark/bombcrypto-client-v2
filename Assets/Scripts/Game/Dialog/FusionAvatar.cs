using System;
using System.Collections.Generic;

using Animation;

using App;

using Senspark;

using Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class FusionAvatar : MonoBehaviour {
        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private Image backlight;

        [SerializeField]
        private GameObject unknownLayer;

        [SerializeField]
        private Button cancelBtn;

        [SerializeField]
        private GameObject groupAmountHeroes;
        
        [SerializeField]
        private Text amountHeroesTxt;

        public int ItemIndex { get; private set; }

        private Action<int> _onSelect;
        private Action<int> _onCancel;
        private ISoundManager _soundManager;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            avatar.gameObject.SetActive(false);
            unknownLayer.gameObject.SetActive(true);
        }

        public void Init(int itemIndex, Action<int> onClicked, Action<int> onCancel) {
            ItemIndex = itemIndex;
            _onSelect = onClicked;
            _onCancel = onCancel;
        }

        public async void SetData(PlayerData playerData) {
            if (cancelBtn) {
                cancelBtn.gameObject.SetActive(playerData != null);
            }
            if (playerData == null) {
                avatar.gameObject.SetActive(false);
                unknownLayer.gameObject.SetActive(true);
                backlight.gameObject.SetActive(false);
                return;
            }
            
            avatar.ChangeImage(playerData);
            avatar.gameObject.SetActive(true);
            unknownLayer.gameObject.SetActive(false);
            backlight.gameObject.SetActive(true);
            backlight.sprite = await AnimationResource.GetBacklightImageByRarity(playerData.rare, true);
        }

        public void SetDatas(List<PlayerData> secondListIdHero) {
            if (secondListIdHero.Count == 0) {
                avatar.gameObject.SetActive(false);
                unknownLayer.gameObject.SetActive(true);
                if (groupAmountHeroes) {
                    groupAmountHeroes.gameObject.SetActive(false);
                }
                return;
            }

            avatar.gameObject.SetActive(true);
            unknownLayer.gameObject.SetActive(false);
            if (groupAmountHeroes) {
                groupAmountHeroes.gameObject.SetActive(true);
                amountHeroesTxt.text = $"{secondListIdHero.Count}";
            }
        }

        public void OnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _onSelect?.Invoke(ItemIndex);
        }

        public void OnCancel() {
            _soundManager.PlaySound(Audio.Tap);
            _onCancel?.Invoke(ItemIndex);
        }
    }
}