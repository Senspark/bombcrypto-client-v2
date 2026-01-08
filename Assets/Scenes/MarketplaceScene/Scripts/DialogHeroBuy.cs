using System;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;

using Senspark;

using Services.Server;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using Utils;

namespace Scenes.MarketplaceScene.Scripts {
    public class DialogHeroBuy : Dialog {
        private UIHeroData _heroData;
        private int _amount;
        private float _unitPrice;

        private IMarketplace _marketplace;
        private IServerManager _serverManager;
        private IAnalytics _analytics;
        private IChestRewardManager _chestRewardManager;
        private ISoundManager _soundManager;

        public Action<bool> OnHideDialogBuy;

        public static UniTask<DialogHeroBuy> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHeroBuy>();
        }
        
        protected override void Awake() {
            base.Awake();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void SetInfo(UIHeroData data, int amount) {
            _heroData = data;
            _amount = amount;
            _unitPrice = data.Price / data.Quantity;
        }

        private bool _isClicked;

        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = false;
            OnBuyButtonClicked();
        }

        public void OnBuyButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var totalPrice = _unitPrice * _amount;
            var bLGem = _chestRewardManager.GetChestReward(BlockRewardType.Gem) + _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            if (bLGem < totalPrice) {
                // DialogOK.ShowInfo(Canvas, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGem,
                    () => {
                        _isClicked = false;
                    });
                return;
            }
            UniTask.Void(async () => {
                try {
                    var product = _heroData.HeroData;
                    var heroIds = await _marketplace.BuyAsync(product, _amount);
                    TrackBuyHeroes(product, heroIds);
                    await _serverManager.General.GetChestReward();
                    OnHideDialogBuy?.Invoke(false);
                    Hide();
                } catch (Exception e) {
                    _analytics.TrackConversion(ConversionType.BuyMarketHeroTrFail);
                    DialogOK.ShowError(DialogCanvas, e.Message, ()=>{_isClicked = false;});
                }
            });
        }

        public void OnNoButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
        
        private void TrackBuyHeroes(ProductHeroData product, int[] heroIds) {
            var productName = product.DataBase.ProductName;
            var productPrice = product.DataBase.Price.Value;
            var (lockGem, gem) = GemUtils.GetGemSpending(_amount * _unitPrice);
            _analytics.MarketPlace_TrackProduct(productName, productPrice, heroIds.Length, MarketPlaceResult.Sold);
            _analytics.MarketPlace_TrackSoftCurrency(
                productName,
                productPrice,
                lockGem,
                gem,
                TrackResult.Done
            );
            _analytics.MarketPlace_TrackBuyHeroTr(product.DataBase.ProductName, heroIds);
            _analytics.TrackConversion(ConversionType.BuyMarketHeroTr);
        }
    }
}