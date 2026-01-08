using System;
using App;
using Data;
using Senspark;
using Engine.Entities;
using Game.Dialog;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLAltarGrindInformation : MonoBehaviour {
        [SerializeField]
        private GameObject content;
        
        [SerializeField]
        private Text nameText;

        [SerializeField]
        private OverlayTexture effectPremium;

        [SerializeField]
        private Avatar avatar;

        [SerializeField]
        private InputField inputQuantity;

        [SerializeField]
        private Text costGoldText;

        [SerializeField]
        private Button minusButton;

        [SerializeField]
        private Button plusButton;

        [SerializeField]
        private Button grindButton;
        
        [SerializeField]
        private GameObject premiumFrame;
        
        private int _maxQuantity;
        private int _quantity;
        private int _unitCost;
        private int _status;

        private int _itemId;
        private IProductItemManager _productItemManager;
        private IChestRewardManager _chestRewardManager;
        private Action<int, int, int, int> _onGrindCallback;

        private void Awake() {
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public void SetOnGrindCallback(Action<int, int, int, int> callback) {
            _onGrindCallback = callback;
        }

        public void SetActive(bool value) {
            content.SetActive(value);
        }
        
        public void UpdateGrind(TRHeroData hero) {
            if (hero == null) {
                _itemId = -1;
                nameText.text = "--";
                avatar.gameObject.SetActive(false);
                costGoldText.text = "--";
                minusButton.interactable = false;
                plusButton.interactable = false;
                grindButton.interactable = false;
                return;
            }

            _itemId = hero.ItemId;
            _unitCost = hero.GrindCost;
            _status = hero.Status;
            nameText.text = _productItemManager.GetItem(hero.ItemId).Name;
            avatar.gameObject.SetActive(true);
            avatar.ChangeImage(UIHeroData.ConvertFromHeroId(hero.ItemId), PlayerColor.HeroTr);

            _maxQuantity = hero.Quantity;
            _quantity = Math.Min(1, _maxQuantity);
            inputQuantity.text = $"{_quantity}";

            minusButton.interactable = true;
            plusButton.interactable = true;
            grindButton.interactable = true;
            UpdateQuantityChange();

            if (effectPremium) {
                var productItem = _productItemManager.GetItem(hero.ItemId);
                effectPremium.enabled = false;
                nameText.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            
            if (premiumFrame) {
                premiumFrame.SetActive(_productItemManager.GetItem(hero.ItemId).ItemKind == ProductItemKind.Premium);
            }
        }

        private void UpdateQuantityChange() {
            if (_quantity > _maxQuantity) {
                _quantity = _maxQuantity;
            } else if (_quantity == 0) {
                _quantity = 1;
            }
            
            minusButton.interactable = _quantity > 1;
            plusButton.interactable = _quantity < _maxQuantity;

            inputQuantity.SetTextWithoutNotify(inputQuantity.text == string.Empty ? string.Empty : $"{_quantity}");
            var costGold = _quantity * _unitCost;
            costGoldText.text = $"{costGold}";

            var gold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            costGoldText.color = gold < costGold ? Color.red :
                ColorUtility.TryParseHtmlString("#FFFC00", out var valueGold) ? valueGold :
                throw new Exception("Wrong Color");
        }
        
        public void OnMinusClicked() {
            _quantity -= 1;
            UpdateQuantityChange();
        }

        public void OnPlusClicked() {
            _quantity += 1;
            UpdateQuantityChange();
        }

        public void OnAllButtonClicked() {
            _quantity = _maxQuantity;
            UpdateQuantityChange();
        }
        
        public void OnInputChange() {
            _quantity = GetValueFromInput(inputQuantity);
            UpdateQuantityChange();
        }
        
        private int GetValueFromInput(InputField input) {
            if (string.IsNullOrEmpty(input.text)) {
                input.text = $"1";
            }
            return int.TryParse(input.text, out var value) ? value : 1;
        }
        
        public void OnGrindButtonClicked() {
            _onGrindCallback(_itemId, _quantity, _quantity * _unitCost, _status);
        }
    }
}