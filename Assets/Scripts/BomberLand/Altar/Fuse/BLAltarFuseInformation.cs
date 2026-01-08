using System;
using App;
using Constant;
using Senspark;
using Game.Dialog.BomberLand.BLGacha;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BLAltarFuseInformation : MonoBehaviour {
        [SerializeField]
        private GameObject content;
        
        [SerializeField]
        private Text nameText;
        
        [SerializeField]
        private Image icon;

        [SerializeField]
        private InputField inputQuantity;

        [SerializeField]
        private Text costGoldText;

        [SerializeField]
        private Text costGemText;
        
        [SerializeField]
        private Button minusButton;

        [SerializeField]
        private Button plusButton;

        [SerializeField]
        private Button fuseButton;

        [SerializeField]
        private BLGachaRes resource;
        
        private int _minQuantity = GameConstant.MinFuseQuantity;
        private int _maxQuantity;
        private int _quantity;
        private int _targetId;
        private int _unitGold;
        private int _unitGem;
        
        private int _itemId;
        private IProductItemManager _productItemManager;
        private IChestRewardManager _chestRewardManager;
        private Action<int, int, int, int, int> _onFuseCallback;

        private void Awake() {
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public void SetOnFuseCallback(Action<int, int, int, int, int> callback) {
            _onFuseCallback = callback;
        }

        public void SetActive(bool value) {
            content.SetActive(value);
        }
        
        public async void UpdateFuse(IUiCrystalData crystal) {
            _itemId = crystal.ItemId;
            _targetId = crystal.TargetItemID;
            _unitGold = crystal.GoldFee;
            _unitGem = crystal.GemFee;
            nameText.text = _productItemManager.GetItem(crystal.ItemId).Name;
            icon.gameObject.SetActive(true);
            icon.sprite = await resource.GetSpriteByItemId(crystal.ItemId);
            _maxQuantity = crystal.Quantity;
            _quantity = Math.Min(_minQuantity, _maxQuantity);
            inputQuantity.text = $"{_quantity}";
            minusButton.interactable = true;
            plusButton.interactable = true;
            fuseButton.interactable = true;
            UpdateQuantityChange();
        }

        private void UpdateQuantityChange() {
            if (_quantity > _maxQuantity - (_maxQuantity % _minQuantity)) {
                _quantity = _maxQuantity - (_maxQuantity % _minQuantity);
            } else if (_quantity == 0) {
                _quantity = 1;
            }
            
            minusButton.interactable = _quantity > _minQuantity;
            plusButton.interactable = _quantity <= _maxQuantity - (_maxQuantity % _minQuantity);
            fuseButton.interactable = _quantity >= _minQuantity;

            inputQuantity.SetTextWithoutNotify(inputQuantity.text == string.Empty ? string.Empty : $"{_quantity}");
            var costGold = _unitGold * (_quantity / _minQuantity);
            var costGem = _unitGem * (_quantity / _minQuantity);
            costGoldText.text = $"{costGold}";
            costGemText.text = $"{costGem}";

            var gold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            var gem = gemUnlock + gemLock;
            costGoldText.color = gold < costGold ? Color.red :
                ColorUtility.TryParseHtmlString("#FFFC00", out var valueGold) ? valueGold :
                throw new Exception("Wrong Color");
            costGemText.color = gem < costGem ? Color.red :
                ColorUtility.TryParseHtmlString("#2AEEFF", out var valueGem) ? valueGem :
                throw new Exception("Wrong Color");
        }
        
        public void OnMinusClicked() {
            _quantity -= _minQuantity;
            UpdateQuantityChange();
        }

        public void OnPlusClicked() {
            _quantity += _minQuantity;
            UpdateQuantityChange();
        }
        
        public void OnAllButtonClicked() {
            _quantity = _maxQuantity - (_maxQuantity % _minQuantity);
            UpdateQuantityChange();
        }
        
        public void OnInputChange() {
            _quantity = GetValueFromInput(inputQuantity);
            _quantity -= (_quantity % _minQuantity);
            UpdateQuantityChange();
        }

        private int GetValueFromInput(InputField input) {
            if (string.IsNullOrEmpty(input.text)) {
                input.text = $"1";
            }
            return int.TryParse(input.text, out var value) ? value : 1;
        }

        public void OnFuseButtonClicked() {
            _onFuseCallback(_itemId, _targetId, _quantity,
                (_quantity / _minQuantity) * _unitGold,
                (_quantity / _minQuantity) * _unitGem);
        }        
    }
}