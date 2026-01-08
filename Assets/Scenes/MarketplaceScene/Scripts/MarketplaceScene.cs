using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Analytics;

using App;

using Controller;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Game.Dialog;
using Game.Manager;
using Game.UI;

using Senspark;

using Services.Server;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MarketplaceScene.Scripts {
    public class MarketplaceScene : BaseTabListScene {
        public static void LoadScene(BLTabType tabPrefer = BLTabType.Heroes) {
            PreferTab = tabPrefer;
            const string sceneName = "MarketplaceScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }
        
        [SerializeField]
        private BLMarketHeroesController heroController;

        [SerializeField]
        private BLMarketController[] controllers;

        [SerializeField]
        private BLBarSort barSort;

        [SerializeField]
        private Image splashFade;
        private BLMarketController _controller;
        private WaitingUiManager _waiting;
        
        private IInputManager _inputManager;    
        private IMarketplace _marketplace;
        private ILogManager _logger;
        private CancellationTokenSource _refreshMinPriceCts;


        protected override void Awake() {
            base.Awake();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _logger = ServiceLocator.Instance.Resolve<ILogManager>();
        }

        protected async void Start() {
            try
            {
                splashFade.gameObject.SetActive(true);
                _waiting = new WaitingUiManager(canvasDialog);
                _waiting.Begin();
                await _marketplace.GetMarketConfig();
                InitUi();
                Analytics.TrackScene(SceneType.VisitMarketplace);
                barSort.SetOnSortUpdate(OnSortUpdate);
                GetChestReward();
                SelectDefaultContent();
                splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
                _refreshMinPriceCts = new CancellationTokenSource();
                RefreshMinPrice(_refreshMinPriceCts.Token).Forget();
                _waiting.End();
            }
            catch (Exception e)
            {
                Debug.LogError("Init MarketplaceScene failed: " + e);
            }
        }

        private void Update() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                OnBackButtonClicked();
            }
        }

        private void OnSortUpdate() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    ClearCacheData(new[] {CurrentContent.Type});
                    await LoadData(CurrentContent.Type);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }
        
        public void OnShowMoreUpdate() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await UpdateData(CurrentContent.Type);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        protected override void ClearCacheData(IEnumerable<BLTabType> tabTypes) {
            heroController.ClearCacheData();
            if (_controller != null) {
                _controller.ClearCacheData();
            }
        }

        protected override async Task LoadData(BLTabType tabType) {
            if (tabType == BLTabType.Heroes) {
                heroController.ClearCacheData();
                await heroController.SetMaxItems();
                heroController.LoadData();
            } else {
                var index = tabType switch {
                    BLTabType.Chest => 0,
                    BLTabType.BombSkin => 1,
                    BLTabType.Booster => 2,
                    BLTabType.Wing => 3,
                    BLTabType.Trail => 4,
                    BLTabType.FireSkin => 5,
                    _ => throw new NotImplementedException()
                };
                _controller = controllers[index];
                _controller.ClearCacheData();
                await _controller.SetMaxItems();
                _controller.LoadData();
            }
        }
        
        private async Task UpdateData(BLTabType tabType) {
            if (tabType == BLTabType.Heroes) {
                heroController.UpdatePageData();
            } else {
                var index = tabType switch {
                    BLTabType.Chest => 0,
                    BLTabType.BombSkin => 1,
                    BLTabType.Booster => 2,
                    BLTabType.Wing => 3,
                    BLTabType.Trail => 4,
                    BLTabType.FireSkin => 5,
                    _ => throw new NotImplementedException()
                };
                _controller = controllers[index];
                _controller.UpdatePageData();
            }
        }

        protected override void ShowDialogItem(ItemData item) {
            DialogItemBuy.Create().ContinueWith(dialog => {
                dialog.SetInfo(item, _controller.GetInputAmount());
                dialog.OnHideDialogBuy = OnHideDialogSellBuy;
                dialog.Show(canvasDialog);
            });
        }

        protected override void ShowDialogHero(UIHeroData hero) {
            DialogHeroBuy.Create().ContinueWith(dialog => {
                dialog.SetInfo(hero, heroController.GetInputAmount());
                dialog.OnHideDialogBuy = OnHideDialogSellBuy;
                dialog.Show(canvasDialog);
            });
        }

        protected override void ShowDialogOrderError() {
            DialogNotificationToShop.ShowOn(canvasDialog, DialogNotificationToShop.Reason.NotEnoughGem);
        }

        protected override void ShowDialogOrder(OrderDataRequest data) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var orderResponse = await _marketplace.OrderItemMarket(data.ItemId, data.Amount, data.Expiration);
                    var dialogOrderItemMarket = await DialogOrderItemMarket.Create();
                    dialogOrderItemMarket.OnHideDialogBuy = OnHideDialogSellBuy;
                    dialogOrderItemMarket.SetInfo(orderResponse);
                    dialogOrderItemMarket.Show(canvasDialog);
                } catch (Exception e) {
                    waiting.End();
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        private void OnDestroy() {
            _refreshMinPriceCts?.Cancel();
            _refreshMinPriceCts?.Dispose();
        }

        private async UniTask RefreshMinPrice(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    await _marketplace.RefreshMinPrice();
                    _logger.Log("Start refresh min price");
                    //Update ui
                    if(_controller != null)
                        _controller.RefreshMinPrice();
                    heroController.RefreshMinPrice();
                    
                    await UniTask.Delay(_marketplace.MinPriceRefreshTime * 1000, cancellationToken: cancellationToken);
                } catch (OperationCanceledException) {
                    _logger.Log("Stop refresh min price");
                    break;
                } catch (Exception e) {
                    _logger.Log("Error when refresh min price: " + e);
                }
            }
        }

    }
}