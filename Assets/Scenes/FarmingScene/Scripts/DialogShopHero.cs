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
        private static DateTime _lastMintTime = DateTime.MinValue;
        private float _cooldownTimer = 0f;

        private int CurrentLevel => GetAccountLevel(_playerStoreManager.GetPlayerCount());
        private int CurrentBulkLimit => GetBulkLimit(CurrentLevel);


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
            
            // Adjust initial index if current limit is lower
            while (_buyHeroIndex > 0 && _buyHeroAmount[_buyHeroIndex] > CurrentBulkLimit) {
                _buyHeroIndex--;
            }

            buttonXs[_buyHeroIndex].SetActive(true);
            RenderPrice(_buyHeroIndex);
            UpdateLevelDisplay();
        }

        private void Update() {
            if (_lastMintTime == DateTime.MinValue) return;
            
            var elapsed = (DateTime.Now - _lastMintTime).TotalSeconds;
            if (elapsed < 60) {
                _cooldownTimer = 60f - (float)elapsed;
                heroTotalSaleLbl.text = $"Cooldown: {_cooldownTimer:F0}s";
            } else {
                UpdateLevelDisplay();
            }
        }

        private void UpdateLevelDisplay()
        {
            var totalMinted = _playerStoreManager.GetPlayerCount();
            var level = AccountLevelHelper.GetAccountLevel(totalMinted);
            var bulkLimit = AccountLevelHelper.GetBulkLimit(level);

            // Prepend Level to the Total Sale label or use a dedicated format
            var totalSale = _storeManager.HeroTotalSale;
            heroTotalSaleLbl.text = $"Level {level} | Global: {totalSale:N0}";

            // Update bulk buttons
            buy5Btn.interactable = bulkLimit >= 5;
            buy10Btn.interactable = bulkLimit >= 10;
            buy15Btn.interactable = bulkLimit >= 15;
            
            // Add visual lock if disabled
            if (buy15Btn.transform.Find("LockIcon") == null && bulkLimit < 15) {
                // Potential to add a lock icon or change color
                // For now, we'll just keep it interactable = false
            }
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
            var limit = CurrentBulkLimit;
            
            amountTitle.text = $"+{buyAmount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";
            
            if (buyAmount > limit) {
                amountTitle.text += $" (Req LVL {buyAmount})";
                heroBcoinPrice.text = "LOCKED";
            } else {
                heroBcoinPrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.Coin * buyAmount);
            }
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
            
            // Check level limit
            if (buyAmount > CurrentBulkLimit) {
                DialogOK.ShowError(DialogCanvas, "Level Restricted", $"Your level {CurrentLevel} only allows buying up to {CurrentBulkLimit} heroes at once.");
                return;
            }

            // Check cooldown
            var elapsed = (DateTime.Now - _lastMintTime).TotalSeconds;
            if (elapsed < 60) {
                DialogOK.ShowError(DialogCanvas, "Cooldown", $"Please wait {60 - elapsed:F0} seconds before next mint.");
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
                            _lastMintTime = DateTime.Now;
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

        private int GetAccountLevel(int totalMinted) {
            if (totalMinted < 150) return 1;
            if (totalMinted < 330) return 2;
            if (totalMinted < 546) return 3;
            if (totalMinted < 805) return 4;
            if (totalMinted < 1116) return 5;
            if (totalMinted < 1489) return 6;
            if (totalMinted < 1936) return 7;
            if (totalMinted < 2473) return 8;
            if (totalMinted < 3115) return 9;
            if (totalMinted < 3885) return 10;
            if (totalMinted < 4809) return 11;
            if (totalMinted < 5918) return 12;
            if (totalMinted < 7249) return 13;
            if (totalMinted < 8846) return 14;
            if (totalMinted < 10762) return 15;
            if (totalMinted < 14700) return 16;
            if (totalMinted < 20840) return 17;
            if (totalMinted < 30420) return 18;
            if (totalMinted < 45360) return 19;
            return 20;
        }

        private int GetBulkLimit(int level) {
            return level >= 15 ? 15 : level;
        }
    }
}