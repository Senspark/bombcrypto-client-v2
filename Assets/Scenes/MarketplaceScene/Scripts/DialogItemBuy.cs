using System;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Services.Server;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using Utils;

namespace Scenes.MarketplaceScene.Scripts {
    public class DialogItemBuy : Dialog {
        private ItemData _itemData;
        private int _amount;
        private float _unitPrice;

        private IAnalytics _analytics;
        private IMarketplace _marketplace;
        private IServerManager _serverManager;
        private IChestRewardManager _chestRewardManager;
        private ISoundManager _soundManager;
        public Action<bool> OnHideDialogBuy;

        public static UniTask<DialogItemBuy> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogItemBuy>();
        }

        protected override void Awake() {
            base.Awake();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void SetInfo(ItemData data, int amount) {
            _itemData = data;
            _amount = amount;
            _unitPrice = data.Price / data.Quantity;
        }

        private bool _isClicked;

        protected override void OnYesClick() {
            if (_isClicked)
                return;
            _isClicked = false;
            OnBuyButtonClicked();
        }

        public void OnBuyButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var totalPrice = _amount * _unitPrice;
            var bLGem = _chestRewardManager.GetChestReward(BlockRewardType.Gem) +
                        _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            if (bLGem < totalPrice) {
                // DialogOK.ShowInfo(Canvas, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGem,
                    () => { _isClicked = false; });
                return;
            }
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var product = _itemData.ProductData;
                    await _marketplace.BuyAsync(product, _amount);
                    TrackBuyItems(product);
                    await _serverManager.General.GetChestReward();
                    OnHideDialogBuy?.Invoke(false);
                    Hide();
                } catch (Exception e) {
                    _analytics.TrackConversion(ConversionType.BuyMarketItemFail);
                    if (e is ErrorCodeException) {
                        await DialogError.ShowError(DialogCanvas, e.Message, ()=>{  _isClicked = false;});
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message, ()=>{  _isClicked = false;});
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnNoButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        private void TrackBuyItems(ProductData product) {
            var productName = product.ProductName;
            var productPrice = product.Price.Value;
            var (lockedGem, gem) = GemUtils.GetGemSpending(_amount * _unitPrice);
            _analytics.MarketPlace_TrackProduct(productName, productPrice, _amount, MarketPlaceResult.Sold);
            _analytics.MarketPlace_TrackSoftCurrency(
                productName,
                productPrice,
                lockedGem,
                gem,
                TrackResult.Done
            );
            _analytics.TrackConversion(ConversionType.BuyMarketItem);
        }
    }
}