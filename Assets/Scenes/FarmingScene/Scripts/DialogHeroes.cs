using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Scenes.FarmingScene.Scripts.CharacterItem;

namespace Scenes.FarmingScene.Scripts {
    public interface IDialogHeroes {
        bool DisableUpgrade { get; set; }
        Dialog OnDidShow(Action action);
        Dialog OnDidHide(Action action);
        void Show(Canvas canvas);
        void SelectItem(int itemIndex);
    }

    public static class DialogHeroesCreator {
        
        public static async UniTask<IDialogHeroes> Create() {
            if (ScreenUtils.IsIPadScreen()) {
                return await DialogHeroesPad.Create();
            }
            return await DialogHeroes.Create();
        }
    }
    
    public class DialogHeroes : Dialog, IDialogHeroes {
        [SerializeField]
        private ScrollRect scroller;

        [SerializeField]
        private GameObject characterPrefab;

        [SerializeField]
        private GameObject characterInfo;

        [SerializeField]
        private Transform characterContain;

        [SerializeField]
        private HeroDetailsDisplay heroDetailsDisplay;

        [SerializeField]
        private GameObject houseConfirm;

        [SerializeField]
        private Text confirmDescription;

        [SerializeField]
        private GameObject heroConfirm;

        [SerializeField]
        private Button upgradeButton;

        [SerializeField]
        private Button workAllButton;

        [SerializeField]
        private Button restAllButton;

        [SerializeField]
        private Button buttonBuy;

        [SerializeField]
        private Text descBuy;
        
        public int SelectIndex { get; set; }
        public float ScrollValue { get; set; }
        public bool DisableUpgrade { get; set; }

        private List<CharacterItem> _items;
        private IPlayerStorageManager _playerStore;
        private IServerManager _serverManager;
        private IFeatureManager _featureManager;
        private IPveHeroStateManager _pveHeroStateManager;
        private IHouseStorageManager _houseStoreManager;
        private IPlayerStorageManager _playerStoreManager;
        private ObserverHandle _handle;
        private ISoundManager _soundManager;
        private IUserSolanaManager _userSolanaManager;

        public static async UniTask<IDialogHeroes> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHeroes>();
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            _houseStoreManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnSyncHero = OnSyncHero,
                OnHeroChangeState = OnHeroChangeState
            });

            DisableUpgrade = !_featureManager.EnableUpgrade;
            heroDetailsDisplay.Init(_featureManager.EnableRepairShield, false);
            App.Utils.ClearAllChildren(scroller.content);
            
            buttonBuy.gameObject.SetActive(_featureManager.EnableClaim);
            descBuy.gameObject.SetActive(!_featureManager.EnableClaim);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle.Dispose();
        }

        public override async void Show(Canvas canvas) {
            base.Show(canvas);
            characterInfo.SetActive(false);
            upgradeButton.gameObject.SetActive(false);
            
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            if (AppConfig.IsSolana()) {
                await _userSolanaManager.GetActiveBomberSol();
            } else {
                await _serverManager.Pve.GetActiveBomber();
            }
            waiting.End();
        }
        
        public Dialog OnDidHide(Action action) {
            base.OnDidHide(action);
            return this;
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        #region PUBLIC METHODS

        public void SelectItem(int itemIndex) {
            if (_items.Count == 0) {
                return;
            }

            itemIndex = Mathf.Clamp(itemIndex, 0, _items.Count - 1);
            ScrollTo(itemIndex);
            OnItemClicked(_items[itemIndex]);
        }

        public void ScrollTo(int itemIndex) {
            var itemCount = _items.Count;
            if (itemCount == 0) {
                return;
            }

            itemIndex = Mathf.Clamp(itemIndex, 0, itemCount - 1);
            var normal = (float) itemIndex / (itemCount - 1);
            scroller.verticalNormalizedPosition = 1f - normal;
        }

        #endregion

        #region EVENT METHODS

        private void OnItemClicked(CharacterItem item) {
            SelectIndex = item.ItemIndex;

            _items.ForEach(e => e.SetChoose(false));

            _items[item.ItemIndex].SetChoose(true);
            heroDetailsDisplay.SetInfo(item.PlayerData, DialogCanvas);
        }

        private void OnItemGoHomeClicked(CharacterItem item) {
            if (!CheckCanGoHome()) {
                return;
            }
            item.HideAllButtons();
            _pveHeroStateManager.ChangeHeroState(item.PlayerData.heroId, HeroStage.Home);
        }

        private void OnItemGoWorkClicked(CharacterItem item) {
            item.HideAllButtons();
            _pveHeroStateManager.ChangeHeroState(item.PlayerData.heroId, HeroStage.Working);
        }

        public void OnItemGoWorkAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var ids = new List<HeroId>();
            foreach (var item in _items) {
                if (item.PlayerData.stage == HeroStage.Working || item.PlayerData.hp < 1) {
                    continue;
                }
                item.HideAllButtons();
                ids.Add(item.PlayerData.heroId);
            }

            if (ids.Count == 0) {
                return;
            }

            workAllButton.gameObject.SetActive(false);
            restAllButton.gameObject.SetActive(false);
            _pveHeroStateManager.ChangeHeroState(ids.ToArray(), HeroStage.Working);
        }

        
        private void OnItemGoSleepClicked(CharacterItem item) {
            item.HideAllButtons();
            _pveHeroStateManager.ChangeHeroState(item.PlayerData.heroId, HeroStage.Sleep);
        }

        public void OnItemGoSleepAllClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var ids = new List<HeroId>();
            foreach (var item in _items) {
                if (item.PlayerData.stage != HeroStage.Working) {
                    continue;
                }
                item.HideAllButtons();
                ids.Add(item.PlayerData.heroId);
            }

            if (ids.Count == 0) {
                return;
            }

            workAllButton.gameObject.SetActive(false);
            restAllButton.gameObject.SetActive(false);
            _pveHeroStateManager.ChangeHeroState(ids.ToArray(), HeroStage.Sleep);
        }

        public void OnBuyHouseClicked() {
            if (!_featureManager.EnableClaim) {
                return;
            }
            DialogShopHouseCreator.Create().ContinueWith(dialog => {
                dialog.Show(DialogCanvas);
                Hide();
            });
        }

        public async void OnBuyHeroClicked() {
            if (!_featureManager.EnableClaim) {
                return;
            }
            var dialog = await DialogShop.Create();
            dialog.Show(DialogCanvas);
            Hide();
        }

        public void OnCloseHeroConfirmClicked() {
            heroConfirm.SetActive(false);
            Hide();
        }

        public void OnCloseHouseConfirmClicked() {
            houseConfirm.SetActive(false);
        }

        public void OnUpgradeButtonClicked() {
            // var d = _items[SelectIndex].PlayerData;
            // var player = _playerStore.GetPlayerDataFromId(d.heroId);
            // var dialog = DialogUpgrade.Create();
            // dialog.Init(player.heroId);
            // dialog.Show(Canvas);
            DialogOK.ShowInfo(DialogCanvas, "Coming soon");
        }

        public void OnBtnControlEnter(BaseEventData data) {
            var pointerEventData = (PointerEventData) data;
            if (pointerEventData == null) {
                return;
            }

            var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
            ability?.ShowTip();
        }

        public void OnBtnControlExit(BaseEventData data) {
            var pointerEventData = (PointerEventData) data;
            if (pointerEventData == null) {
                return;
            }

            var ability = pointerEventData.pointerEnter.GetComponent<AbilityItem>();
            ability?.HideTip();
        }

        public void OnButtonHide() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        #endregion

        #region PRIVATE METHODS

        private void InstantiateItems(bool scroll) {
            foreach (Transform child in characterContain) {
                Destroy(child.gameObject);
            }

            _items ??= new List<CharacterItem>();
            _items.Clear();

            if (_playerStore.GetPlayerCount() == 0) {
                heroConfirm.SetActive(true);
                upgradeButton.gameObject.SetActive(false);
                return;
            }
            if (_playerStore.GetActivePlayersAmount() == 0) {
                characterInfo.SetActive(false);
                return;
            }
            
            heroConfirm.SetActive(false);
            upgradeButton.gameObject.SetActive(!DisableUpgrade);

            var characterItemCallback = new CharacterItemCallback {
                OnClicked = OnItemClicked,
                OnGoHome = OnItemGoHomeClicked,
                OnGoWork = OnItemGoWorkClicked,
                OnGoSleep = OnItemGoSleepClicked,
            };

            // Sắp xếp danh sách theo trạng thái và id, phải lấy ra i (index) để init cho CharacterItem.
            var sortedPlayers = _playerStore.GetActivePlayerData()
                .OrderBy(e => e.stage)
                .ThenBy(e => e.heroId);

            foreach (var h in sortedPlayers) {
                var obj = Instantiate(characterPrefab, characterContain, false);
                var item = obj.GetComponent<CharacterItem>();
                item.SetInfo(h, _items.Count, characterItemCallback);
                _items.Add(item);
            }

            characterInfo.SetActive(true);
            StartCoroutine(ScrollToCoroutine(scroll));

            var workCount = _items.Count(e => e.PlayerData.stage == HeroStage.Working);
            var restCount = _items.Count - workCount;
            workAllButton.gameObject.SetActive(restCount > 0);
            restAllButton.gameObject.SetActive(workCount > 0);
        }

        private IEnumerator ScrollToCoroutine(bool scroll) {
            if (_items.Count == 0) {
                yield break;
            }

            yield return null;
            yield return null;

            SelectIndex = Mathf.Clamp(SelectIndex, 0, _items.Count - 1);
            if (scroll) {
                if (ScrollValue > 0) {
                    scroller.verticalNormalizedPosition = ScrollValue;
                } else {
                    ScrollTo(SelectIndex);
                }
            }

            OnItemClicked(_items[SelectIndex]);
        }

        private bool CheckCanGoHome() {
            var houseStore = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
            var isCanGoHome = true;
            if (houseStore.GetHouseCount() <= 0) {
                confirmDescription.text = "You need to buy a house\nto use this function";
                isCanGoHome = false;
            } else {
                var slot = houseStore.GetHouseSlot();
                var inHomes = _playerStore.GetHomePlayerCount();
                if (inHomes >= slot) {
                    confirmDescription.text = "You need to buy\nbigger house";
                    isCanGoHome = false;
                }
            }

            if (!isCanGoHome) {
                //houseConfirm.SetActive(true);
                return false;
            }

            return true;
        }

        private void OnSyncHero(ISyncHeroResponse _) {
            var scrollValue = Mathf.Clamp01(scroller.verticalNormalizedPosition);
            InstantiateItems(true);
            ScrollValue = scrollValue;
        }

        private void OnHeroChangeState(IPveHeroDangerous obj) {
            InstantiateItems(false);
        }

        private void ShowErrorNoHouse(string errorCode) {
            int.TryParse(errorCode, out var ec);
            confirmDescription.text = ec == 1008
                ? "You need to buy\nbigger house"
                : ErrorCode.ErrorDescription(ec);
            //houseConfirm.SetActive(true);
        }

        #endregion
    }
}