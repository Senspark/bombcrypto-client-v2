using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Manager;
using Game.UI;
using Reconnect;
using Reconnect.Backend;
using Reconnect.View;
using Scenes.ShopScene.Scripts;
using Scenes.StoryModeScene.Scripts;
using Senspark;
using Services;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scenes.AltarScene.Scripts {
    public class AltarScene : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private BLLeftMenuTab tabList;

        [SerializeField]
        private Transform contentList;

        [SerializeField]
        private BLTabType defaultTab;
        
        [FormerlySerializedAs("heroController")]
        [SerializeField]
        private BLAltarGrindController grindController;
        
        [SerializeField]
        private BLAltarFuseController fuseController;

        [SerializeField]
        private Image splashFade;
        
        private IServerManager _serverManager;
        private IServerRequester _serverRequester;
        private ISoundManager _soundManager;
        private IProductItemManager _productItemManager;
        private ILogManager _logManager;
        private IAnalytics _analytics;
        private IChestRewardManager _chestRewardManager;
        private IReconnectStrategy _reconnectStrategy;

        private Dictionary<BLTabType, BLAltarBaseContent> _dictContent;
        private BLAltarBaseContent _currentContent;

        private void Awake() {
            splashFade.gameObject.SetActive(true);
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _reconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
            waiting.End();
            splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
        }

        private void OnDestroy() {
            _reconnectStrategy.Dispose();
        }

        private void Start() {
            tabList.SetOnSelectCallback(OnSelectMenuTab);
            _dictContent = new Dictionary<BLTabType, BLAltarBaseContent>();
            var contents = contentList.GetComponentsInChildren<BLAltarBaseContent>();
            foreach (var content in contents) {
                content.SetGrindCallback(ShowDialogGrind);
                content.SetFuseCallback(ShowDialogFuse);
                _dictContent.Add(content.Type, content);
            }
            SetDefaultContent();
        }

        public void SetDefaultTab(BLTabType tabType) {
            defaultTab = tabType;
            SetDefaultContent();
        }

        private void SetDefaultContent() {
            foreach (var content in _dictContent.Values) {
                content.SetSelected(content.Type == defaultTab);
                if (content.Type == defaultTab) {
                    tabList.ForceSelected(defaultTab);
                }
            }
        }

        private void OnSelectMenuTab(BLTabType tabType) {
            _soundManager.PlaySound(Audio.Tap);

            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    _currentContent = _dictContent[tabType];
                    await LoadData(tabType);
                    _currentContent.SetSelected(true);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
                    _logManager.Log(e.Message);
                } finally {
                    waiting.End();
                }
            });
        }
        
        private async Task LoadData(BLTabType tabType) {
            switch (tabType) {
                case BLTabType.Grind:
                    grindController.OnShowDialogInfo = () => ShowDialogInfo(canvasDialog, tabType);
                    await grindController.LoadData();
                    break;
                case BLTabType.Fuse:
                    fuseController.OnShowDialogInfo = () => ShowDialogInfo(canvasDialog, tabType);
                    await fuseController.LoadData();
                    break;
            }
        }

        private void RefreshList(BLTabType tabType) {
            OnSelectMenuTab(tabType);
        }
        
        private void ShowDialogGrind(int itemId, int quantity, int cost, int status) {
            var itemName = _productItemManager.GetItem(itemId).Name;

            DialogGrind.Create().ContinueWith(dialog => {
                dialog.SetInfo(itemName, quantity, cost,
                    () => DoGrind(itemId, quantity, status, cost));
                dialog.Show(canvasDialog);
            });
        }

        private void ShowDialogFuse(int itemId, int targetId, int quantity, int costGold, int costGem) {
            var fromName = _productItemManager.GetItem(itemId).Name;
            var toName = _productItemManager.GetItem(targetId).Name;
            var unitQuantity = quantity / GameConstant.MinFuseQuantity;
            DialogFuse.Create().ContinueWith(dialog=> {
                dialog.SetInfo(itemId, fromName, quantity, targetId, toName, unitQuantity, 
                    costGem, costGold,
                    () => DoFuse(itemId, quantity, costGold, costGem));
                dialog.Show(canvasDialog);
            });
        }

        private void DoGrind(int itemId, int quantity, int status, int cost) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _serverRequester.GrindHero(itemId, quantity, status);
                    TrackCreateCrystal(itemId, quantity, result, cost);
                    await _serverManager.General.GetChestReward();
                    ShowDialogReward(result, BLTabType.Grind);
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

        private void TrackCreateCrystal(int itemId, int quantity, (int, int)[] rewards, int gold) {
            var heroName =  _productItemManager.GetItem(itemId).Name;
            var crap = 0;
            var lesser = 0;
            var rough = 0;
            var pure = 0;
            var perfect = 0;
            foreach (var (id, amount) in rewards) {
                var productId = (GachaChestProductId) id;
                switch (productId) {
                    case GachaChestProductId.Scrap:
                        crap = amount;       
                        break;
                    case GachaChestProductId.LesserCrystal:
                        lesser = amount;
                        break;
                    case GachaChestProductId.RoughCrystal:
                        rough = amount;
                        break;
                    case GachaChestProductId.PureCrystal:
                        pure = amount;
                        break;
                    case GachaChestProductId.PerfectCrystal:
                        perfect = amount;
                        break;
                }
            }
            _analytics.TrackCreateCrystal(heroName, quantity, crap, lesser, rough, pure, perfect, gold);
        }

        private void DoFuse(int itemId, int quantity, int gold, int gem) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _serverRequester.UpgradeCrystal(itemId, quantity);
                    TrackUpgradeCrystal(itemId, quantity,
                        result.ItemId, result.Quantity,
                        gold, gem
                    );
                    await _serverManager.General.GetChestReward();
                    var reward = new (int, int)[] {result};
                    ShowDialogReward(reward, BLTabType.Fuse);
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

        private void TrackUpgradeCrystal(int rawItemId, int rawAmount,
            int targetItemId, int targetAmount,
            int gold, int gem) {
            var rawName = _productItemManager.GetItem(rawItemId).Name;
            var targetName = _productItemManager.GetItem(targetItemId).Name;

            var gemLock = 0;
            var gemUnlock = 0;
            var chestGemLock = (int) _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);

            if (chestGemLock >= gem) {
                gemLock = gem;
                gemUnlock = 0;
            } else {
                gemLock = chestGemLock;
                gemUnlock = gem - gemLock;
            }

            _analytics.TrackUpgradeCrystal(rawName, rawAmount, targetName, targetAmount, gold, gemLock, gemUnlock);
        }
        
        private void ShowDialogReward((int itemId, int quantity)[] rewards, BLTabType tabType) {
            LuckyWheelReward.GetDialogLuckyReward(rewards.Length).ContinueWith((dialogReward) => {
                    dialogReward.UpdateUI(rewards.Select(it => (it.itemId, it.quantity)));
                    dialogReward.OnDidHide(() => RefreshList(tabType));
                    dialogReward.Show(canvasDialog);
                }
            );
        }
        
        public void OnBackButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }
        
        private void ShowDialogInfo(Canvas canvasDialog, BLTabType tabType) {
            switch (tabType) {
                case BLTabType.Grind:
                    ShowGrindInfo(canvasDialog);
                    break;
                case BLTabType.Fuse:
                    ShowFuseInfo(canvasDialog);
                    break;
                default: {
                    break;
                }
            }
        }

        private void ShowGrindInfo(Canvas canvasDialog) {
            DialogChestInfo.Create().ContinueWith((dialog) => {
                dialog.SetTitle("GRIND INFORMATION")
                    .SetDescription(
                        "Grind Hero soul into Crystals.")
                    .Show(canvasDialog);
            });
        }

        private void ShowFuseInfo(Canvas canvasDialog) {
            DialogChestInfo.Create().ContinueWith((dialog) => {
                dialog.SetTitle("FUSE INFORMATION")
                    .SetDescription(
                        "You need to have at least 4 crystals of the same type to fuse into higher level crystals.")
                    .Show(canvasDialog);
            });
        }
    }
}