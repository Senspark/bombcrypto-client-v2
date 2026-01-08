using System;
using Analytics;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services;
using Services.Server;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.InventoryScene.Scripts {
    public class DialogHeroSell : Dialog {
        [SerializeField]
        private Text heroName;

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
        private Button sellButton;

        [SerializeField]
        private Text maxPriceText;

        private UIHeroData _itemData;
        private ItemMarketConfig _itemMarketConfig;
        private int _itemId;
        private int _quantity;
        private float _marketPrice;
        private int _amount;
        private int _price;
        private int _maxSellPrice;

        private IMarketplace _marketplace;
        private IAnalytics _analytics;
        private ISoundManager _soundManager;
        private IProductItemManager _productItemManager;
        private IProductManager _productManager;

        public Action<bool> OnHideDialogSell;

        public static UniTask<DialogHeroSell> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHeroSell>();
        }
        
        protected override void Awake() {
            base.Awake();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _productManager = ServiceLocator.Instance.Resolve<IProductManager>();
        }

        public void SetInfo(UIHeroData data) {
            _itemData = data;
            _itemId = data.HeroId;
            _quantity = data.Quantity;
            _marketPrice = data.Price;
            _maxSellPrice = Mathf.RoundToInt(_productManager.GetProduct(_itemId).GetMaxGemPriceShop() *
                                             GameConstant.RatioPriceShop);
            maxPriceText.text = $"max <color=#37ff00>{_maxSellPrice}</color> each";
            if (_marketPrice <= 0) {
                _marketPrice = 1;
            }

            inputAmount.text = $"{_quantity}";
            inputPrice.text = $"{_marketPrice}";
            UpdateReceive();
            heroName.text = $"{data.HeroName}";
            avatar.ChangeAvatar(data);
            if (!effectPremium) {
                return;
            }
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            var productItem = _productItemManager.GetItem(data.HeroId);
            effectPremium.enabled = false;
            heroName.color = productItem.ItemKind == ProductItemKind.Premium
                ? effectPremium.m_OverlayColor : Color.white;
        }

        public void OnAmountAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            inputAmount.text = $"{_quantity}";
            UpdateReceive();
        }

        public void OnPriceAutoClicked() {
            inputPrice.text = $"{_marketPrice}";
            UpdateReceive();
        }

        public void OnInputChange() {
            UpdateReceive();
        }

        protected override void OnYesClick() {
            if (sellButton.IsInteractable()) {
                OnSellButtonClicked();
            }
        }

        public void OnSellButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            sellButton.interactable = false;
            UniTask.Void(async () => {
                try {
                    await _marketplace.SellItemMarket(_itemId, _price, _amount, (int)InventoryItemType.Hero, 0);
                    _analytics.MarketPlace_TrackProduct(_itemData.HeroName, _price, _amount,
                        MarketPlaceResult.BeginSell);
                    OnHideDialogSell?.Invoke(true);
                    Hide();
                } catch (Exception e) {
#if UNITY_EDITOR
                    Debug.LogException(e);
#endif
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                } finally {
                    sellButton.interactable = true;
                }
            });
        }

        private int GetValueFromInput(InputField input) {
            return int.TryParse(input.text, out var value) ? value : 0;
        }

        private void UpdateReceive() {
            var amount = GetValueFromInput(inputAmount);
            var price = GetValueFromInput(inputPrice);

            amount = Mathf.Clamp(amount, 1, _quantity);
            // price = Mathf.Clamp(price, 1, _maxSellPrice);
            price = Mathf.Clamp(price, 1, int.MaxValue);
            if (MathUtils.MultiplyNotOverflow(amount, price, out var result)) {
                _amount = amount;
                _price = price;
                receiveText.text = $"{result}";
            }

            inputAmount.SetTextWithoutNotify(inputAmount.text == string.Empty ? string.Empty : $"{_amount}");
            inputPrice.SetTextWithoutNotify(inputPrice.text == string.Empty ? string.Empty : $"{_price}");
            sellButton.interactable = !(inputAmount.text == string.Empty || inputPrice.text == string.Empty);
        }
    }
}