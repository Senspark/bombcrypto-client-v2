using System;

using Cysharp.Threading.Tasks;

using Data;
using Engine.Entities;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLAltarGrindItem : MonoBehaviour {
        [SerializeField]
        private Text nameText;

        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private Text valueText;

        [SerializeField]
        private Image highLight;

        [SerializeField]
        private Image lockIcon;
        
        private int _index;
        private Action<int> _onClickCallback;
        private IProductItemManager _productItemManager;

        public void SetInfo(int index, TRHeroData hero, Action<int> callback) {
            _index = index;
            _onClickCallback = callback;
            UniTask.Void(async () => {
                await avatar.ChangeImage(UIHeroData.ConvertFromHeroId(hero.ItemId), PlayerColor.HeroTr);
            });
            valueText.text = $"x{hero.Quantity}";
            
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            nameText.text = _productItemManager.GetItem(hero.ItemId).Name;
            if (effectPremium) {
                var productItem = _productItemManager.GetItem(hero.ItemId);
                effectPremium.enabled = false;
                nameText.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            lockIcon.gameObject.SetActive(hero.Status == 1);
        }

        public void SetSelected(bool value) {
            highLight.gameObject.SetActive(value);
        }

        public void OnClicked() {
            _onClickCallback?.Invoke(_index);
        }
        
        public void SetInvisible(bool value) {
            gameObject.SetActive(value);
        }
    }
}