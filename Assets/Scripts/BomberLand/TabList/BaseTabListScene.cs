using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;
using App;

using Cysharp.Threading.Tasks;

using Senspark;
using Game.Dialog;
using Game.Manager;
using Reconnect;
using Reconnect.Backend;
using Reconnect.View;

using Scenes.MainMenuScene.Scripts;

using Services.Server;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public abstract class BaseTabListScene : MonoBehaviour {
        [SerializeField]
        protected Canvas canvasDialog;

        [SerializeField]
        protected BLLeftMenuTab tabList;

        [SerializeField]
        private Transform contentList;

        [SerializeField]
        private BLTabType defaultTab;
        
        [SerializeField]
        private Button showMoreButton;

        private ISoundManager _audioManager;
        private IReconnectStrategy _reconnectStrategy;

        protected IAnalytics Analytics;
        protected IServerManager ServerManager { get; private set; }
        protected IServerRequester ServerRequester { get; private set; }
        protected Dictionary<BLTabType, BaseBLContent> DictContent;
        protected BaseBLContent CurrentContent;

        protected static BLTabType PreferTab = BLTabType.Null;
        
        protected virtual void Awake() {
            _audioManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            ServerManager = ServiceLocator.Instance.Resolve<IServerManager>();
            ServerRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            Analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _reconnectStrategy = new DefaultReconnectStrategy(
                ServiceLocator.Instance.Resolve<ILogManager>(),
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
        }

        private void OnDestroy() {
            _reconnectStrategy.Dispose();
        }

        protected void InitUi() {
            tabList.SetOnSelectCallback(OnSelectMenuTab);
            DictContent = new Dictionary<BLTabType, BaseBLContent>();
            var contents = contentList.GetComponentsInChildren<BaseBLContent>();
            foreach (var content in contents) {
                content.OnShowDialogItem = ShowDialogItem;
                content.OnShowDialogHero = ShowDialogHero;
                content.OnShowDialogOrder = ShowDialogOrder;
                content.OnOrderErrorCallback = ShowDialogOrderError;
                DictContent.Add(content.Type, content);
            }
        }

        protected void SelectDefaultContent() {
            if (PreferTab != BLTabType.Null) {
                defaultTab = PreferTab;
            }
            foreach (var content in DictContent.Values) {
                content.SetSelected(content.Type == defaultTab);
                if (content.Type == defaultTab) {
                    tabList.ForceSelected(defaultTab);
                }
            }
        }

        protected void GetChestReward() {
            ServerManager.General.GetChestReward();
        }

        protected abstract void ShowDialogItem(ItemData item);
        protected abstract void ShowDialogHero(UIHeroData hero);
        protected abstract void ShowDialogOrder(OrderDataRequest data);
        protected abstract void ShowDialogOrderError();

        protected void OnHideDialogSellBuy(bool isSell) {
            var title = isSell ? "Item Listed" : "Success";
            var description =
                isSell ? "Item listed successfully\nYou can check in the On Sell Tab" : "Buy successfully";

            // clear cache data of OnSell and CurrentContent
            ClearCacheData(new[] {BLTabType.OnSell, CurrentContent.Type});

            DialogOK.ShowInfoAsync(canvasDialog, description, new DialogOK.Optional {
                Title = title,
                OnDidHide = RefreshList
            }).Forget();
        }

        private void RefreshList() {
            OnSelectMenuTab(CurrentContent.Type);
        }

        private void OnSelectMenuTab(BLTabType tabType) {
            _audioManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    CurrentContent = DictContent[tabType];
                    CurrentContent.SetEnableShowMore(EnableShowMore);
                    CurrentContent.SetSelected(true);
                    await LoadData(tabType);
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(canvasDialog, e.Message);    
                    } else {
                        DialogOK.ShowError(canvasDialog, e.Message);
                    }
#if UNITY_EDITOR
                    Debug.LogException(e);
#endif
                } finally {
                    waiting.End();
                }
            });
        }

        private void EnableShowMore(bool state) {
            showMoreButton.gameObject.SetActive(state);
        }

        public void OnBackButtonClicked() {
            _audioManager.PlaySound(Audio.Tap);
            void OnMainMenuLoaded(GameObject obj) {
                var mainMenu = obj.GetComponent<MainMenuScene>();
                if (defaultTab == BLTabType.Avatar) {
                    mainMenu.ShowProfile();
                }
            }
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName, OnMainMenuLoaded).Forget();
        }
        protected abstract void ClearCacheData(IEnumerable<BLTabType> tabTypes);
        protected abstract Task LoadData(BLTabType tabType);
    }
}