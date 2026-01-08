using System;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.UI;

using Senspark;

using Services;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.InventoryScene.Scripts {
    public class DialogItemEdit : Dialog {
        private bool _isClicked;

        
        public static UniTask<DialogItemEdit> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogItemEdit>();
        }

        [SerializeField]
        private Text itemName;
        
        [SerializeField]
        private OverlayTexture effectPremium;

        [SerializeField]
        private BLTablListAvatar avatar;

        [SerializeField]
        private InputField inputAmount;

        [SerializeField]
        private InputField inputPrice;

        [SerializeField]
        private Text receiveText;

        [SerializeField]
        private Button editButton;

        private int _itemId;
        private int _quantity;
        private float _unitPrice;
        private float _marketPrice;
        private int _amount;
        private int _price;

        public Action<bool> OnHideDialogSell;
        
        private Action <float, float, int, int>_onEditPrice = null;

        private ISoundManager _soundManager;
        private IProductItemManager _productItemManager;
        
        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }
        
        public void SetInfo(ItemData data) {
            _itemId = data.ItemId;
            _quantity = data.Quantity;
            _unitPrice = data.UnitPrice;
            _marketPrice = data.Price;
            inputAmount.text = $"{_quantity}";
            inputPrice.text = $"{_unitPrice}";
            UpdateReceive();
            itemName.text = $"{data.ItemName}";
            avatar.ChangeAvatar(data);
            if (!effectPremium) {
                return;
            }
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            var productItem = _productItemManager.GetItem(data.ItemId);
            effectPremium.enabled = false;
            itemName.color = productItem.ItemKind == ProductItemKind.Premium
                ? effectPremium.m_OverlayColor : Color.white;
        }
        
        public void SetOnEditPrice(Action<float, float, int, int> onEditPrice) {
            _onEditPrice = onEditPrice;
        }

        public void OnAmountAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            inputAmount.text = $"{_quantity}";
            UpdateReceive();
        }

        public void OnPriceAutoClicked() {
            _soundManager.PlaySound(Audio.Tap);
            inputPrice.text = $"{_marketPrice}";
            UpdateReceive();
        }

        public void OnInputChange() {
            UpdateReceive();
        }
        
        protected override void OnYesClick() {
            if(_onEditPrice == null)
                return;
            if(_isClicked)
                return;
            _isClicked = true;
            OnSellButtonClicked();
        }

        public void OnSellButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (_onEditPrice != null) {
                _onEditPrice.Invoke(_unitPrice, _price, _quantity, _amount);
            }
        }
        
        private int GetValueFromInput(InputField input) {
            return int.TryParse(input.text, out var value) ? value : 0;
        }
        
        private void UpdateReceive() {
            var amount = GetValueFromInput(inputAmount);
            var price = GetValueFromInput(inputPrice);

            amount = Mathf.Clamp(amount, 1, _quantity);
            // price = Mathf.Clamp(price, 1, GameConstant.MaxPriceInput);
            price = Mathf.Clamp(price, 1, int.MaxValue);
            
            if (MathUtils.MultiplyNotOverflow(amount, price, out var result)) {
                _amount = amount;
                _price = price;
                receiveText.text = $"{result}";
            }

            inputAmount.SetTextWithoutNotify(inputAmount.text == string.Empty ? string.Empty : $"{_amount}");
            inputPrice.SetTextWithoutNotify(inputPrice.text == string.Empty ? string.Empty : $"{_price}");
            editButton.interactable = !(inputAmount.text == string.Empty || inputPrice.text == string.Empty);
        }
    }
}