using System;

using Animation;
using App;
using Data;
using Engine.Components;
using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand {
    public class AnimationAvatar : MonoBehaviour {
        [SerializeField]
        private Text userName;

        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private ImageAnimation icon;

        [SerializeField]
        private ImageAnimation wing;
        
        [SerializeField]
        private GameObject premiumFrame;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private AnimationResource animationResource;

        private System.Action _onHeroClickedCallback;
        private IDialogManager _dialogManager;
        [SerializeField] [CanBeNull] private Dialog currentDialogOpenMe;

        private void Start() {
            _dialogManager = ServiceLocator.Instance.Resolve<IDialogManager>();
        }

        public void HideHero() {
            icon.gameObject.SetActive(false);
            wing.gameObject.SetActive(false);
        }

        public void SetClickedCallback(System.Action callback) {
            _onHeroClickedCallback = callback;
        }

        public void ShowHero(PlayerData player, ProductItemData productItem) {
            if (userName && effectPremium) {
                userName.text = productItem.Name;
                effectPremium.enabled = false;
                userName.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            if (premiumFrame) {
                premiumFrame.SetActive(productItem.ItemKind == ProductItemKind.Premium);
            }
            PlayAnimationHero(player);
            icon.gameObject.SetActive(true);
        }

        private void HideWing() {
            wing.gameObject.SetActive(false);
        }

        public void ShowWing(int itemId) {
            if (itemId < 0) {
                HideWing();
                return;
            }
            PlayAnimation(wing, itemId);
            wing.gameObject.SetActive(true);
        }

        private void PlayAnimationHero(PlayerData player) {
            var sprites = animationResource.GetSpriteIdle(player.playerType, player.playercolor, FaceDirection.Down);
            icon.StartLoop(sprites);
        }

        private async void PlayAnimation(ImageAnimation imageAnimation, int itemId) {
            var resourcePicker =  await resource.GetAnimationByItemId(itemId);
            var type = resourcePicker.Type;
            if (type > 0 && resourcePicker.AnimationIdle.Length > 0) {
                imageAnimation.StartLoop(resourcePicker.AnimationIdle);
            }
        }
        public void OnChoose() {
            if (_dialogManager.IsAnyDialogOpened(currentDialogOpenMe)) {
                return;
            }
            OnHeroClicked();

        }
        

        public void OnHeroClicked() {
            _onHeroClickedCallback?.Invoke();
        }
    }
}