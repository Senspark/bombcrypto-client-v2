using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;
using App;
using BomberLand.Button;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Senspark;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Scenes.FarmingScene.Scripts {
    public class DialogShopHero : Dialog {
        [SerializeField]
        private List<GameObject> heroSObjects;

        [SerializeField]
        private Text amountTitle;
        
        [SerializeField]
        private Text heroBcoinPrice;

        [SerializeField]
        private XButton[] buttonXs;
        
        [SerializeField]
        private Text heroTotalSaleLbl;

        private int _buyHeroIndex = 0;
        private readonly int[] _buyHeroAmount = {1, 5, 10, 15};

        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private IPlayerStorageManager _playerStoreManager;
        private ILanguageManager _languageManager;
        private IBlockchainManager _blockchainManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IAnalytics _analytics;
        private IServerManager _serverManager;
        
        private Action<int> _buyCallback;
        private double _bcoinPrice;
        private bool _isHeroS;
        private bool _isClicked;


        public static UniTask<DialogShopHero> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHero>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            
            var totalSale = _storeManager.HeroTotalSale;
            heroTotalSaleLbl.text = $"{totalSale:N0}";
        }

        public void Init(bool isHeroS) {
            _isHeroS = isHeroS;
            heroSObjects.ForEach(e => e.SetActive(isHeroS));
            buttonXs[_buyHeroIndex].SetActive(true);
            RenderPrice(_buyHeroIndex);
        }

        public void OnXButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _buyHeroIndex = (_buyHeroIndex + 1) % _buyHeroAmount.Length;
            RenderPrice(_buyHeroIndex);
        }
        public void OnXButtonClicked(XButton button) {
            _soundManager.PlaySound(Audio.Tap);
            foreach (var iter in buttonXs) {
                if (iter == button) {
                    iter.SetActive(true);
                    _buyHeroIndex = iter.Index;
                    RenderPrice(_buyHeroIndex);
                } else {
                    iter.SetActive(false);
                }
            }
        }

        
        private void RenderPrice(int index) {
            var buyAmount = _buyHeroAmount[index];
            amountTitle.text = $"+{buyAmount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";
            heroBcoinPrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.Coin * buyAmount);
        }

        public void OnBuyWithBcoinBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var buyAmount = _buyHeroAmount[_buyHeroIndex];
            BuyHero(buyAmount, BuyHeroCategory.WithBcoin);
        }
        
        private void BuyHero(int buyAmount, BuyHeroCategory category) {
            _soundManager.PlaySound(Audio.Tap);

            if (!CheckEnoughResource(buyAmount)) {
                TrackBuyHeroFail();
                return;
            }
            if (!CheckLimit(buyAmount)) {
                TrackBuyHeroFail();
                return;
            }

            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(DialogCanvas);
                waiting.ShowLoadingAnim();
                
                try {
                    var processToken = await ProcessTokenHelper.GetPendingHero(DialogCanvas, _blockchainManager);
                    var buyError = false;

                    if (await _blockchainManager.BuyHero(buyAmount, category, _isHeroS)) {
                        _analytics.TrackConversion(ConversionType.BuyHeroFi);
                        processToken.pendingHeroes += await ProcessTokenHelper.WaitForPendingHero(processToken, _blockchainManager);
                        await SyncNewCoinBalance(category);
                    } else {
                        TrackBuyHeroFail();
                        buyError = true;
                    }

                    // Process token requests.
                    if (processToken.pendingHeroes > 0) {
                        var onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();
                        onBoardingManager.DispatchEvent(e => e.updateOnBoarding?.Invoke(TutorialStep.DoneBuyHero));
                        waiting.ChangeText(_languageManager.GetValue(LocalizeKey.info_process_token));

                        var result = await ProcessTokenHelper.ProcessTokenRequest(DialogCanvas, _blockchainManager,
                            _serverManager, true, true);

                        if (result) {
                            Hide();
                        } else {
                            throw new Exception("Claim Failed");
                        }
                    } else if (!buyError) {
                        throw new Exception("Buy Failed");
                    }
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message, () => { _isClicked = false;});    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message, () => { _isClicked = false;});
                    }
                } finally {
                    waiting.Hide();
                }
            });
        }

        private async Task SyncNewCoinBalance(BuyHeroCategory category) {
            if (category == BuyHeroCategory.WithBcoin) {
                await _blockchainManager.GetBalance(RpcTokenCategory.Bcoin);
            }
        }

        private bool CheckEnoughResource(int buyAmount) {
            var isEnoughCoin = _blockchainStorageManager.GetBalance(BlockRewardType.BCoin) >=
                               _storeManager.HeroPrice.Coin * buyAmount;
            if (isEnoughCoin) {
                return true;
            }
            var t = _languageManager.GetValue(LocalizeKey.ui_not_enough);
            var d = _languageManager.GetValue(LocalizeKey.info_not_enough_resource);
            DialogOK.ShowInfo(DialogCanvas, t, d, new DialogOK.Optional{OnDidHide = () => { _isClicked = false;}});
            return false;
        }

        private bool CheckLimit(int buyAmount) {
            var playerLimit = _storeManager.HeroLimit;
            var playerCount = _playerStoreManager.GetPlayerCount();

            if (playerCount + buyAmount > playerLimit) {
                var tit = _languageManager.GetValue(LocalizeKey.ui_hero_limit);
                var desc = string.Format(_languageManager.GetValue(LocalizeKey.info_cant_buy_heroes), playerLimit);
                DialogOK.ShowInfo(DialogCanvas, tit, desc, new DialogOK.Optional{OnDidHide = () => { _isClicked = false;}});
                return false;
            }
            return true;
        }

        private void TrackBuyHeroFail() {
            _analytics.TrackConversion(ConversionType.BuyHeroFiFail);
        }

        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnBuyWithBcoinBtnClicked();
        }
    }
}