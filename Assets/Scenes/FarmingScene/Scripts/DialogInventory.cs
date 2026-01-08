using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using App;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DynamicScrollRect;
using Game.Dialog;
using Game.Manager;
using JetBrains.Annotations;
using Scenes.TreasureModeScene.Scripts.Dialog;
using Senspark;
using Services.WebGL;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using StoryMode.Component;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public interface IDialogInventory {
        Dialog OnDidHide(Action action);
        void ShowLockHero();
        void Init(DialogInventory.SortRarity sortRarity, int targetUpgradeRarity, bool enableRepair);
        void SetChooseHeroForInventoryFusion(PlayerData[] heroesSelected, HeroId[] excludeHeroIds,
            Action<PlayerData[]> callBackBurnHeroId);
        void SetChooseHeroForPvp(Action<HeroId> onSelectAsPvpCallback);
        void SetOnHideBySelf(Action onHideBySelf);
        void SetChooseHeroForPreviewSummary();
        void Show(Canvas canvas, string titleDialog = "INVENTORY");
        void Show(Canvas canvas, DialogInventory.GetPlayer getPlayer, string titleDialog = "INVENTORY");
        void SetChooseHeroForResetRoi(HeroId[] excludeHeroIds, Action<HeroId> onSelectedCallback);
        void SetChooseHeroForInventoryBurnHero(HeroId[] excludeHeroIds, Action<PlayerData[]> callBackBurnHeroId);
        void SetChooseHeroForResetSkill(Action<HeroId> onSelectedCallback);
        void SetChooseHeroForUpgrade(HeroId baseHeroId, int baseHeroLevel, Action<HeroId> onSelectAsMaterialCallback);
    }
    
    public static class DialogInventoryCreator {
        public static async UniTask<IDialogInventory> Create() {
            if (ScreenUtils.IsIPadScreen()) {
                return await DialogInventoryPad.Create();
            } 
            return await DialogInventory.Create();
        }

         public static async UniTask<IDialogInventory> CreateForFusion() {
             if (ScreenUtils.IsIPadScreen()) {
                 return await DialogInventoryPad.CreateForFusion();
             } 
             return await DialogInventory.CreateForFusion();
         }
     }
     public class DialogInventory : Dialog, IDialogInventory {
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private DynamicScrollRect.DynamicScrollRect scroller;

        [SerializeField]
        protected DynamicInventoryItem inventoryItemPrefab;

        [SerializeField]
        protected HeroDetailsDisplay heroDescriptionPanel;

        [SerializeField]
        protected HeroDetailsDisplay heroDescriptionPopup;

        [SerializeField]
        protected List<Button> activeButtons;

        [SerializeField]
        protected List<Button> deactiveButtons;

        [SerializeField]
        protected List<Button> upgradeButtons;

        [SerializeField]
        protected List<Button> chooseMaterialButtons;

        [SerializeField]
        protected List<LockCountdown> unlockNextDay;

        [SerializeField]
        protected Dropdown dropDown1;

        [SerializeField]
        protected Dropdown dropDown2;

        [SerializeField]
        protected Dropdown dropDownHeroFilter;

        [SerializeField]
        protected Dropdown dropDownActiveFilter;

        [SerializeField]
        protected Button btnRepair;

        [SerializeField]
        protected Button btnChoose;

        [SerializeField]
        protected Transform container;

        [SerializeField]
        protected GameObject pageContent;

        [SerializeField]
        protected TMP_InputField inputField;

        [SerializeField]
        protected TMP_Text numPages;

        [CanBeNull]
        [SerializeField]
        private TouchScreenKeyboardWebgl keyboard;

        [CanBeNull]
        [SerializeField]
        private RectTransform dialogTransform;
        
        [CanBeNull]
        [SerializeField]
        private Text capacityText;
        
        [CanBeNull]
        [SerializeField]
        private GameObject groupButton;
        
        [CanBeNull]
        [SerializeField]
        private GameObject notSupportedText;

        public delegate List<PlayerData> GetPlayer();
        
        //DevHoang_20250612: Only for DialogInventoryAirdropForFusion
        [SerializeField] [CanBeNull]
        private GameObject selectAllBtn;
        private Action<bool> _onSelectAll;
        

        public float ScrollValue { get; set; } = -1;
        public HeroId SelectId { get; set; } = default;
        public Vector2Int ColumnRow { get; set; }
        public SortOrder1 Order1 { get; set; } = SortOrder1.ActiveFirst;
        public SortOrder2 Order2 { get; set; } = SortOrder2.HighStatsFirst;
        public HeroTypeFilter Filter1 { get; set; } = HeroTypeFilter.AllHeroesType;
        public ActiveFilter FilterActive { get; set; } = ActiveFilter.All;

        private IServerManager _serverManager;
        private IPlayerStorageManager _playerStore;

        private List<DynamicInventoryItem> _items;
        private ChooseMode _chooseMode = ChooseMode.None;
        private Sequence _timeOutTween;
        private WaitingUiManager _waiting;
        private CancellationTokenSource _uiTaskCancellation;
        private ObserverHandle _handle;
        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private IUserSolanaManager _userSolanaManager;

        private List<PlayerData> _heroesIdBurn = new();
        private List<int> _heroesBurnIds = new();

        private SortRarity _sortRarity = SortRarity.Default;
        private int _targetUpgradeRarity; // Fusion hero
        public static int MaxSelectChooseHero;

        private Action<int[]> _callBackBurnHeroId;
        private GetPlayer _getPlayer;
        private bool _notHover;
        private bool _isShowLockHero = false;
        private Action _onHideBySelf;

        private const int MaxNumForDynamic = 200;
        private readonly DynamicScroll<DynamicInventoryItem, DynamicObject> _verticalDynamicScroll = new();

        private const int PageRow = 8;
        private int _itemsInPage = 50;
        private int _currentPage;
        private int _totalPages;
        private List<int> _totalHeroIds = new List<int>();

        private float _originalBottom;

        public static async UniTask<DialogInventory> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInventory>();
        }

        public static async UniTask<DialogInventory> CreateForFusion() {
            if (AppConfig.IsAirDrop()) {
                var dialog = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                    .Instantiate<DialogInventoryAirdropForFusion>();
                return dialog;
            }
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInventory>();
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
            _webGlUtils = ServiceLocator.Instance.Resolve<IWebGLBridgeUtils>();
            _uiTaskCancellation = new CancellationTokenSource();
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnSyncHero = OnSyncHero
            });
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var enableRepair = featureManager.EnableRepairShield;
            heroDescriptionPanel.Init(enableRepair);
            heroDescriptionPopup.Init(enableRepair);

            if (AppConfig.IsAirDrop()) {
                dropDownHeroFilter.gameObject.SetActive(false);
            } else {
                dropDownHeroFilter.gameObject.SetActive(featureManager.EnableBHero && featureManager.EnableBHeroS);
            }
            keyboard?.gameObject.SetActive(false);
            if (dialogTransform != null)
                _originalBottom = dialogTransform.offsetMin.y;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            EndWait();
            _uiTaskCancellation.Cancel();
            _uiTaskCancellation.Dispose();
            _handle.Dispose();
        }

        protected void InitForFusion(Action<bool> onSelectAll) {
            _onSelectAll = onSelectAll;
        }

        private void UpdatePageText(string text) {
            if (!int.TryParse(text, out var iKeyboard)) {
                return;
            }
            var valid = Math.Clamp(iKeyboard, 1, _totalPages);
            if (int.TryParse(inputField.text, out var amount)) {
                if (amount != valid) {
                    inputField.text = $"{valid}";
                }
            } else {
                inputField.text = $"{valid}";
            }
        }

        public async void Show(Canvas canvas, string titleDialog = "INVENTORY") {
            if (title != null)
                title.text = titleDialog;
            base.Show(canvas);
            pageContent.SetActive(false);
            scroller.gameObject.SetActive(false);

            // mặc định không load player list và sort
            // mà get trực tiếp sorted player data.
            // _getPlayer ??= () => _playerStore.GetPlayerDataList(HeroAccountType.Nft, HeroAccountType.Ton,HeroAccountType.Trial);

            BeginWait();
            heroDescriptionPopup.Hide();
            heroDescriptionPanel.HideInfo();
            HideButtons();
            SetDataToDropDown();

            _currentPage = 0;
            inputField.text = $"{_currentPage + 1}";

            foreach (Transform child in scroller.content.transform) {
                Destroy(child.gameObject);
            }
            // Chờ dialog FadeIn
            // (Nếu không hiệu ứng FadeIn sẽ bị mất khi InstantiateItems lâu hơn 0.5s)
            await UniTask.Delay(500);
            await InstantiateItems(true, _sortRarity, Order1, Order2);
            scroller.gameObject.SetActive(true);

            EndWait();

            // tắt sort active thay bằng filter active
            dropDown1.gameObject.SetActive(false);

            //tắt filter active hero nếu là dialog tổng kết
            dropDownActiveFilter.gameObject.SetActive(_chooseMode != ChooseMode.PreviewSummary);

            inputField.onValueChanged.AddListener(OnPageChange);

            if (selectAllBtn != null) {
                selectAllBtn.SetActive(_chooseMode == ChooseMode.InventoryFusion);
            }
        }

        public void Show(Canvas canvas, GetPlayer getPlayer, string titleDialog = "INVENTORY") {
            _getPlayer = getPlayer;
            Show(canvas, titleDialog);
        }

        protected override void OnYesClick() {
            //Do nothing
        }

        #region PUBLIC METHODS

        public void Init(SortRarity sortRarity, int targetUpgradeRarity, bool enableRepair) {
            _targetUpgradeRarity = targetUpgradeRarity;
            _sortRarity = sortRarity;
            heroDescriptionPanel.Init(enableRepair);
            heroDescriptionPopup.Init(enableRepair);
        }

        public void Init(bool enableRepair, bool showGroupButton, bool enablePreviewStake) {
            heroDescriptionPanel.Init(enableRepair, showGroupButton, enablePreviewStake);
        }

        private async UniTask InstantiateItems(
            bool scroll,
            SortRarity rarity,
            SortOrder1 order1 = SortOrder1.ActiveFirst,
            SortOrder2 order2 = SortOrder2.HighStatsFirst,
            HeroTypeFilter filter1 = HeroTypeFilter.AllHeroesType,
            ActiveFilter filterActive = ActiveFilter.All) {
            // Tính số column của grid theo chiểu ngang của view port 
            var gridLayoutGroup = scroller.content.GetComponent<GridLayoutGroup>();
            var rect = scroller.viewport.rect;
            var width = rect.width;
            var itemWidth = gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x;
            var column = (int)(width / itemWidth);
            _itemsInPage = column * PageRow;

            List<PlayerData> players;
            if (_getPlayer == null) {
                // Get sorted data có ExcludeHeroIds trước khi Phân trang 
                players = await GetSortedPlayerData(rarity, _targetUpgradeRarity, order1, order2, filter1, filterActive);
            } else {
                // Trường hợp _getPlayer != null là
                // trường hợp hiển thị danh sách new heroes, hiển thị cho trường hợp mua bằng max option, có phân trang
                // và không sort Active
                players = SortPlayerData(_getPlayer(), rarity, _targetUpgradeRarity, order1, order2,
                    filter1, filterActive);
            }
            
            var num = players.Count;
            if (capacityText != null) {
                var heroCount = _playerStore.GetTotalHeroesSize();
                capacityText.text = $"{heroCount}/{_storeManager.HeroLimit}";
            }
            _items = new List<DynamicInventoryItem>();
            var inventoryItemCallback = new InventoryItem.InventoryItemCallback();
            if (_chooseMode == ChooseMode.InventoryBurn) {
                inventoryItemCallback.OnClicked = OnItemClickedInventoryBurn;
            } else if (_chooseMode == ChooseMode.InventoryFusion) {
                inventoryItemCallback.OnClicked = OnItemClickedInventoryFusion;
            } else {
                inventoryItemCallback.OnClicked = OnItemClicked;
            }
            inventoryItemCallback.OnHover = OnItemHover;

            var parent = scroller.content.transform;
            if (num > MaxNumForDynamic) {
                parent = container;
            } else {
                foreach (Transform child in parent) {
                    Destroy(child.gameObject);
                }
            }

            // Cập nhật _heroesIdBurn theo instance player mới
            for (var i = 0; i < num; i++) {
                var player = players[i];
                if (!_heroesBurnIds.Contains(player.heroId.Id)) {
                    continue;
                }
                for (var j = 0; j < _heroesIdBurn.Count; j++) {
                    if (_heroesIdBurn[j] != null &&
                        _heroesIdBurn[j].heroId.Id == player.heroId.Id) {
                        _heroesIdBurn[j] = player;
                    }
                }
            }
            var curActiveFilter = ConvertFromValue(dropDownActiveFilter.value);
            var isIOSBrowser = await _webGlUtils.IsIOSBrowser();
            if (curActiveFilter == ActiveFilter.Locked && isIOSBrowser) {
                scroller.content.gameObject.SetActive(false);
                if (notSupportedText != null) {
                    notSupportedText.SetActive(true);
                }
                return;
            }

            for (var i = 0; i < num; i++) {
                if (players[i] != null) {
                    scroller.content.gameObject.SetActive(true);
                    if (notSupportedText != null) {
                        notSupportedText.SetActive(false);
                    }
                    var item = Instantiate(inventoryItemPrefab, parent, false);
                    _heroesIdBurn = await item.SetInfo(players[i], inventoryItemCallback, _chooseMode, _heroesIdBurn,
                        _chooseMode == ChooseMode.PvpMode,
                        _heroesIdBurn.Contains(players[i]), canvas: DialogCanvas, heroDescriptionPanel);
                    item.UpdateLockedHeroes(curActiveFilter == ActiveFilter.Locked);
                    if (_isShowLockHero) {
                        item.UpdateUILockHero(players[i]);
                    }
                    _items.Add(item);
                }
            }

            if (_chooseMode == ChooseMode.Upgrade) {
                FilterHeroesSuitableToUpgrade();
            }

            // ExcludeHeroes sẽ thực hiện trong phần GetSortedData
            // (exclude trước khi phân trang)
            //FilterExcludeHeroes();

            if (num > MaxNumForDynamic) {
                StartCoroutine(InitDynamicScroll());
            }
            StartCoroutine(GetRowAndColumn());
            StartCoroutine(ScrollToCoroutine(scroll));

            // Tự chọn hero đầu tiên trong ds
            if (_items.Count > 0) {
                OnItemClicked(_items[0].Item);
            }
            foreach (var iter in _items) {
                iter.Item.OnSelectAllItemClicked(_heroesBurnIds.Contains(iter.Item.playerData.heroId.Id));
            }
        }

        public void SelectItem(int itemIndex) {
            if (_items.Count == 0) {
                return;
            }

            itemIndex = Mathf.Clamp(itemIndex, 0, _items.Count - 1);
            ScrollTo(itemIndex);
            OnItemClicked(_items[itemIndex].Item);
        }

        public void ScrollTo(int itemIndex) {
            var itemCount = _items.Count;
            if (itemCount == 0 || ColumnRow.x == 0 || ColumnRow.y == 0) {
                return;
            }

            if (_verticalDynamicScroll.Initiated) {
                _verticalDynamicScroll.MoveToIndex(itemIndex); //, 2);
            } else {
                itemIndex = Mathf.Clamp(itemIndex, 0, itemCount - 1);
                var row = (itemIndex) / ColumnRow.x;
                var normal = (float)row / (ColumnRow.y - 1);
                scroller.verticalNormalizedPosition = 1f - normal;
            }
        }

        public Dialog OnDidHide(Action action) {
            base.OnDidHide(action);
            return this;
        }

        public void ShowLockHero() {
            _isShowLockHero = true;
        }

        #endregion

        #region EVENT METHODS

        private void OnItemClicked(InventoryItem item) {
            if (_verticalDynamicScroll.Initiated) {
                _verticalDynamicScroll.SelectId = item.playerData.heroId.Id;
            }
            UpdateItemInfo(item);
            if(_chooseMode != ChooseMode.InventoryFusion && _chooseMode != ChooseMode.InventoryBurn)
                SetHighLight(item);
            _notHover = true;
        }

        private void SetHighLight(InventoryItem item) {
            foreach (var iter in _items) {
                iter.SetHighLight(iter.Item.playerData.heroId.Id == item.playerData.heroId.Id);
            }
        }

        private void OnItemClickedInventoryBurn(InventoryItem item) {
            UpdateItemInfo(item);
            var playerData = item.playerData;
            if (item.IsSelectItem) {
                _heroesIdBurn.Add(playerData);
            } else {
                _heroesIdBurn.Remove(playerData);
            }
        }

        private void OnItemClickedInventoryFusion(InventoryItem item) {
            UpdateItemInfo(item);
            var playerData = item.playerData;
            if (item.IsSelectItem) {
                //Add giá trị vào đầu list
                _heroesBurnIds.Add(playerData.heroId.Id);
                _onSelectAll?.Invoke(_heroesBurnIds.Count >= _totalHeroIds.Count);
                for (var i = 0; i < _heroesIdBurn.Count; i++) {
                    if (_heroesIdBurn[i] == null) {
                        _heroesIdBurn[i] = playerData;
                        return;
                    }
                }
                _heroesIdBurn.Add(playerData);
            } else {
                //Add giá trị vào đầu list
                _heroesBurnIds.Remove(SelectId.Id);
                _onSelectAll?.Invoke(false);
                for (var i = 0; i < _heroesIdBurn.Count; i++) {
                    if (_heroesIdBurn[i] != null && _heroesIdBurn[i].heroId == SelectId) {
                        _heroesIdBurn[i] = null;
                        return;
                    }
                }
            }
        }

        private void OnItemHover(InventoryItem item) {
            if (_notHover) {
                return;
            }
            UpdateItemInfo(item);
        }

        private void UpdateItemInfo(InventoryItem item) {
            var playerData = item.playerData;
            SelectId = playerData.heroId;
            heroDescriptionPanel.SetInfo(playerData, DialogCanvas, HideButtonsAndPanels);
            ShowButtons(playerData, _chooseMode != ChooseMode.None);
            if (_isShowLockHero) {
                heroDescriptionPanel.SetLockHero(() => { ShowButtons(playerData, _chooseMode != ChooseMode.None); });
            }
        }

        protected void OnSelectAllItems(bool isSelectAll) {
            if (isSelectAll) {
                foreach (var id in _totalHeroIds) {
                    if (!_heroesBurnIds.Contains(id)) {
                        _heroesBurnIds.Add(id);
                    }
                }
                _heroesIdBurn.Clear();
                var heroAccountType = HeroAccountType.Nft;
                //DevHoang: Add new airdrop
                if (AppConfig.IsTon()) {
                    heroAccountType = HeroAccountType.Ton;
                } else if (AppConfig.IsSolana()) {
                    heroAccountType = HeroAccountType.Sol;
                } else if (AppConfig.IsRonin()) {
                    heroAccountType = HeroAccountType.Ron;
                } else if (AppConfig.IsBase()) {
                    heroAccountType = HeroAccountType.Bas;
                } else if (AppConfig.IsViction()) {
                    heroAccountType = HeroAccountType.Vic;
                }
                var allPlayerData = _playerStore.GetPlayerDataList(heroAccountType);
                foreach (var iter in allPlayerData) {
                    if (_heroesBurnIds.Contains(iter.heroId.Id)) {
                        _heroesIdBurn.Add(iter);
                    }
                }
            } else {
                _heroesBurnIds.Clear();
                _heroesIdBurn.Clear();
            }
            foreach (var iter in _items) {
                iter.Item.OnSelectAllItemClicked(_heroesBurnIds.Contains(iter.Item.playerData.heroId.Id));
            }
        }

        public void OnPopupClosedClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            heroDescriptionPopup.Hide();
        }

        public void OnActiveClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);

            if (_playerStore.GetActivePlayersAmount() >= 15) {
                var str = "Hero max active reach";
                DialogOK.ShowError(DialogCanvas, str);
                return;
            }

            BeginWait();
            UniTask.Void(async () => {
                try {
                    HideButtonsAndPanels();
                    if (AppConfig.IsSolana()) {
                        await _userSolanaManager.ActiveBomberSol(SelectId, 1);
                    } else {
                        await _serverManager.Pve.ActiveBomber(SelectId, 1);
                    }
                    if (_uiTaskCancellation.IsCancellationRequested) {
                        return;
                    }
                    SortInventory();
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    EndWait();
                }
            });
        }

        public void OnDeactiveClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            BeginWait();
            UniTask.Void(async () => {
                HideButtonsAndPanels();
                if (AppConfig.IsSolana()) {
                    await _userSolanaManager.ActiveBomberSol(SelectId, 0);
                } else {
                    await _serverManager.Pve.ActiveBomber(SelectId, 0);
                }
                if (_uiTaskCancellation.IsCancellationRequested) {
                    return;
                }
                SortInventory();
                EndWait();
            });
        }

        public void OnUpgradeBtnClicked() {
            // Hiện tại trong FeatureManager đang set EnableUpgrade = false
            // Và DialogUpgrade (prefab và script) đá xóa => sau này nếu bật lại true => thì cần phải thiết kế lại DialogUpgrade
            throw new System.NotImplementedException("Upgrade chưa được thiết lập");
        }

        private async void OnSyncHero(ISyncHeroResponse _) {
            if (capacityText != null) {
                var heroCount = _playerStore.GetTotalHeroesSize();
                capacityText.text = $"{heroCount}/{_storeManager.HeroLimit}";
            }
            var scrollValue = Mathf.Clamp01(scroller.verticalNormalizedPosition);
            var sortOrder1 = (SortOrder1)dropDown1.value;
            var sortOrder2 = (SortOrder2)dropDown2.value;
            var filter1 = (HeroTypeFilter)dropDownHeroFilter.value;
            var filterActive = ConvertFromValue(dropDownActiveFilter.value);
            await InstantiateItems(true, _sortRarity, sortOrder1, sortOrder2, filter1, filterActive);
            ScrollValue = scrollValue;
        }

        public void OnCloseBtnClicked() {
            _onHideBySelf?.Invoke();
            _onHideBySelf = null;
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        public void SetOnHideBySelf(Action onHideBySelf) {
            _onHideBySelf = onHideBySelf;
        }

        #endregion

        #region PRIVATE METHODS

        private void HideButtonsAndPanels() {
            heroDescriptionPopup.Hide();
            heroDescriptionPanel.HideInfo();
            HideButtons();
        }

        private IEnumerator ScrollToCoroutine(bool scroll) {
            if (_items.Count == 0) {
                yield break;
            }

            yield return null;
            yield return null;

            var index = _items.FindIndex(e => e.Item.playerData.heroId == SelectId);
            var validIndex = index >= 0 && index < _items.Count;

            if (scroll) {
                if (ScrollValue >= 0) {
                    scroller.verticalNormalizedPosition = ScrollValue;
                } else if (validIndex) {
                    ScrollTo(index);
                } else {
                    ScrollTo(1);
                }
            }

            // if (validIndex)
            //     OnItemClicked(_items[index]);
        }

        private void BeginWait() {
            EndWait();
            _waiting ??= new WaitingUiManager(DialogCanvas);
            _waiting.Begin();
        }

        private void EndWait() {
            if (_waiting == null) {
                return;
            }
            _waiting.End();
            _waiting = null;
        }

        private IEnumerator InitDynamicScroll() {
            // Fix Me: để tạm 1 item prefab trong editor 1 frame trước khi xóa
            // Mục đích đẩy anchoredPosition.y của content = 0 khi đang ở top.
            // dễ dàng cho việc tính giới hạn top khi kéo xuống.
            yield return null;

            // xóa các items tạm trước khi tạo item thực
            if (!_verticalDynamicScroll.Initiated) {
                foreach (Transform child in scroller.content.transform) {
                    Destroy(child.gameObject);
                }
            }
            _verticalDynamicScroll.InitiateGrid(scroller, _items, 0, container);
        }

        private IEnumerator GetRowAndColumn() {
            yield return null;
            var grid = scroller.content.GetComponent<GridLayoutGroup>();
            ColumnRow = GridLayoutGroupUtil.GetColumnAndRow(grid);
        }

        private void HideButtons() {
            activeButtons.ForEach(e => e.gameObject.SetActive(false));
            deactiveButtons.ForEach(e => e.gameObject.SetActive(false));
            upgradeButtons.ForEach(e => e.gameObject.SetActive(false));
            chooseMaterialButtons.ForEach(e => e.gameObject.SetActive(false));
        }

        private void ShowButtons(PlayerData playerData, bool forChooseOnly) {
            if (forChooseOnly) {
                activeButtons.ForEach(e => e.gameObject.SetActive(false));
                deactiveButtons.ForEach(e => e.gameObject.SetActive(false));
                upgradeButtons.ForEach(e => e.gameObject.SetActive(false));

                chooseMaterialButtons.ForEach(e => {
                    e.gameObject.SetActive(_chooseMode != ChooseMode.PreviewSummary);
                    e.interactable = _chooseMode switch {
                        ChooseMode.StoryModePlayToEarn => !playerData.storyIsPlayed,
                        ChooseMode.Upgrade => playerData.level < 5,
                        ChooseMode.PvpMode => playerData.battery > 0,
                        _ => true
                    };
                });

                if (_chooseMode == ChooseMode.StoryModePlayToEarn) {
                    unlockNextDay.ForEach(e => {
                        e.gameObject.SetActive(playerData.storyIsPlayed);
                        e.SetTimeUnlock(playerData.timeUnlock);
                    });
                } else {
                    unlockNextDay.ForEach(e => e.gameObject.SetActive(false));
                }
            } else {
                activeButtons.ForEach(e => e.gameObject.SetActive(!playerData.active));
                deactiveButtons.ForEach(e => e.gameObject.SetActive(playerData.active));
                var enableUpgrade = ServiceLocator.Instance.Resolve<IFeatureManager>().EnableUpgrade;
                upgradeButtons.ForEach(e => {
                    e.gameObject.SetActive(enableUpgrade);
                    e.interactable = enableUpgrade;
                });
                unlockNextDay.ForEach(e => { e.gameObject.SetActive(false); });

                chooseMaterialButtons.ForEach(e => e.gameObject.SetActive(false));
            }
        }

        #endregion

        #region SORTING

        #region Player Data Sort

        public enum SortRarity {
            Default,
            BelowOneLevel,
            BelowThanOneLevel
        }

        public enum SortOrder1 {
            ActiveFirst,
            UnActiveFirst
        }

        public enum SortOrder2 {
            HighStatsFirst,
            HighRarityFirst,
            NewestFirst
        }

        public enum ActiveFilter {
            All,
            Selected,
            Active,
            UnActive,
            Locked
        }

        public enum HeroTypeFilter {
            AllHeroesType,
            HeroOnly,
            HeroSOnly
        }

        private void SetDataToDropDown() {
            dropDown1.options.Clear();
            dropDown2.options.Clear();
            dropDownHeroFilter.options.Clear();
            dropDownActiveFilter.options.Clear();

            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();

            dropDown1.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_active)));
            dropDown1.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_unactive)));

            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_high_stats)));
            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_high_rarity)));
            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_newest)));

            dropDownHeroFilter.options.Add(
                new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_filter_all_heroes)));
            dropDownHeroFilter.options.Add(
                new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_filter_bhero_only)));
            dropDownHeroFilter.options.Add(
                new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_filter_heros_only)));

            dropDownActiveFilter.options.Add(
                new Dropdown.OptionData("All"));
            if (_chooseMode == ChooseMode.InventoryFusion) {
                dropDownActiveFilter.options.Add(
                    new Dropdown.OptionData("Selected"));
            }
            dropDownActiveFilter.options.Add(
                new Dropdown.OptionData("Active"));
            dropDownActiveFilter.options.Add(
                new Dropdown.OptionData("UnActive"));
            if (_chooseMode != ChooseMode.InventoryFusion && (AppConfig.IsTon() || AppConfig.IsSolana())) {
                dropDownActiveFilter.options.Add(
                    new Dropdown.OptionData(ActiveFilter.Locked.ToString()));
            }

            //Update text dropdown
            dropDown1.captionText.text = dropDown1.options[0].text;
            dropDown2.captionText.text = dropDown2.options[0].text;
            dropDownHeroFilter.captionText.text = dropDownHeroFilter.options[0].text;
            dropDownActiveFilter.captionText.text = dropDownActiveFilter.options[0].text;

            dropDown1.value = (int)Order1;
            dropDown2.value = (int)Order2;
            dropDownHeroFilter.value = (int)Filter1;
            dropDownActiveFilter.value = (int)FilterActive;

            dropDown1.onValueChanged.AddListener(_ => SortInventory());
            dropDown2.onValueChanged.AddListener(_ => SortInventory());
            dropDownHeroFilter.onValueChanged.AddListener(_ => SortInventory());
            dropDownActiveFilter.onValueChanged.AddListener(_ => SortInventory());
        }

        private async void SortInventory() {
            _currentPage = 0;
            inputField.text = $"{_currentPage + 1}";
            var curActiveFilter = ConvertFromValue(dropDownActiveFilter.value);
            if (groupButton != null) {
                groupButton.SetActive(curActiveFilter != ActiveFilter.Locked);
            }
            await InstantiateItems(false, _sortRarity, (SortOrder1)dropDown1.value, (SortOrder2)dropDown2.value,
                (HeroTypeFilter)dropDownHeroFilter.value, curActiveFilter);
        }

        private ActiveFilter ConvertFromValue(int value) {
            return value switch {
                0 => ActiveFilter.All,
                1 => _chooseMode == ChooseMode.InventoryFusion ? ActiveFilter.Selected : ActiveFilter.Active,
                2 => _chooseMode == ChooseMode.InventoryFusion ? ActiveFilter.Active : ActiveFilter.UnActive,
                3 => _chooseMode == ChooseMode.InventoryFusion ? ActiveFilter.UnActive : ActiveFilter.Locked,
                _ => throw new ArgumentOutOfRangeException($"ActiveFilter Value", value, null),
            };
        }

        private async UniTask<List<PlayerData>> GetSortedPlayerData(
            SortRarity orderRarity,
            int targetRarity,
            SortOrder1 order1,
            SortOrder2 order2,
            HeroTypeFilter order3,
            ActiveFilter filterActive) {
            if (filterActive == ActiveFilter.Locked && _playerStore.GetLockedHeroesData() == null) {
                var isIos = await _webGlUtils.IsIOSBrowser();
                if(!isIos) {
                    await _serverManager.General.GetHeroOldSeason();
                }
            }
            var (list, totalPages, totalHeroIds) = _playerStore.GetSortedPlayerDataList(orderRarity, targetRarity,
                order1, order2, order3, filterActive,
                _heroesBurnIds.ToArray(),
                _excludeHeroIds,
                _currentPage, _itemsInPage,
                //DevHoang: Add new airdrop
                HeroAccountType.Nft, HeroAccountType.Ton, HeroAccountType.Trial, 
                HeroAccountType.Sol, HeroAccountType.Ron, HeroAccountType.Bas, HeroAccountType.Vic
            );

            _totalPages = totalPages;
            _totalHeroIds = totalHeroIds;
            numPages.text = $"/{_totalPages}";
            pageContent.SetActive(_totalPages > 1);

            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(DefaultPlayerStoreManager.GeneratePlayerData(t));
            }
            if (_totalHeroIds.Count == 0) {
                _onSelectAll?.Invoke(false);
            } else {
                _onSelectAll?.Invoke(_heroesBurnIds.Count >= _totalHeroIds.Count);
            }
            return result;
        }

        private List<PlayerData> SortPlayerData(
            List<PlayerData> input,
            SortRarity orderRarity,
            int targetRarity,
            SortOrder1 order1,
            SortOrder2 order2,
            HeroTypeFilter order3,
            ActiveFilter filterActive
        ) {
            var (list, totalPages, totalHeroIds) = _playerStore.GetSortedPlayerDataList(input, orderRarity, targetRarity,
                order2, order3,
                _currentPage, _itemsInPage);

            _totalPages = totalPages;
            _totalHeroIds = totalHeroIds;
            numPages.text = $"/{_totalPages}";
            pageContent.SetActive(_totalPages > 1);
            return list;
        }

        #endregion

        #endregion

        #region FOR UPGRADE

        private Action<HeroId> _onSelectAsMaterialCallback;
        private HeroId _baseHeroId;
        private int _baseHeroLevel;
        private HeroId[] _excludeHeroIds;
        private IWebGLBridgeUtils _webGlUtils;

        /// <summary>
        /// Cho phép tận dụng Dialog Inventory để chọn Heroes Material phục vụ cho Dialog Upgrade
        /// </summary>
        public void SetChooseHeroForUpgrade(HeroId baseHeroId, int baseHeroLevel,
            Action<HeroId> onSelectAsMaterialCallback) {
            _chooseMode = ChooseMode.Upgrade;
            _baseHeroId = baseHeroId;
            _baseHeroLevel = baseHeroLevel;
            _onSelectAsMaterialCallback = onSelectAsMaterialCallback;
            SelectId = default;
            ScrollValue = -1;
        }

        public void SetChooseHeroForStory(Action<HeroId> onSelectAsStoryCallback, bool playToEarn) {
            _chooseMode = playToEarn ? ChooseMode.StoryModePlayToEarn : ChooseMode.StoryModePlayForFun;
            _baseHeroId = default;
            _baseHeroLevel = 0;
            _onSelectAsMaterialCallback = onSelectAsStoryCallback;
            SelectId = default;
            ScrollValue = -1;
        }

        public void SetChooseHeroForPvp(Action<HeroId> onSelectAsPvpCallback) {
            _chooseMode = ChooseMode.PvpMode;
            _baseHeroId = default;
            _baseHeroLevel = 0;
            _onSelectAsMaterialCallback = onSelectAsPvpCallback;
            SelectId = default;
            ScrollValue = -1;
        }

        public void SetChooseHeroForResetSkill(Action<HeroId> onSelectedCallback) {
            _chooseMode = ChooseMode.ResetSkill;
            _baseHeroId = default;
            _baseHeroLevel = 0;
            _onSelectAsMaterialCallback = onSelectedCallback;
            SelectId = default;
            ScrollValue = -1;
        }

        public void SetChooseHeroForResetRoi(HeroId[] excludeHeroIds, Action<HeroId> onSelectedCallback) {
            _chooseMode = ChooseMode.ResetRoi;
            _baseHeroId = default;
            _baseHeroLevel = 0;
            _excludeHeroIds = excludeHeroIds;
            _onSelectAsMaterialCallback = onSelectedCallback;
            SelectId = default;
            ScrollValue = -1;
        }

        public void SetChooseHeroForInventoryBurnHero(HeroId[] excludeHeroIds,
            Action<PlayerData[]> callBackBurnHeroId) {
            _chooseMode = ChooseMode.InventoryBurn;
            _excludeHeroIds = excludeHeroIds;
            OnWillHide(() => { callBackBurnHeroId?.Invoke(_heroesIdBurn.ToArray()); });
            UpdateUIInventoryBurnHero();
        }

        public void SetChooseHeroForInventoryFusion(PlayerData[] heroesSelected, HeroId[] excludeHeroIds,
            Action<PlayerData[]> callBackBurnHeroId) {
            _chooseMode = ChooseMode.InventoryFusion;

            _heroesBurnIds = heroesSelected.Where(e => e != null).Select(e => e.heroId.Id).ToList();
            _heroesIdBurn = heroesSelected.ToList();

            _excludeHeroIds = excludeHeroIds;
            OnWillHide(() => { callBackBurnHeroId?.Invoke(_heroesIdBurn.ToArray()); });
            UpdateUIInventoryBurnHero();
        }

        public void SetChooseHeroForPreviewSummary() {
            _chooseMode = ChooseMode.PreviewSummary;
            heroDescriptionPanel.Init(false, false, false);
            btnChoose.gameObject.SetActive(_chooseMode != ChooseMode.PreviewSummary);
        }

        private void UpdateUIInventoryBurnHero() {
            btnChoose.gameObject.SetActive(_chooseMode != ChooseMode.InventoryBurn ||
                                           _chooseMode != ChooseMode.InventoryFusion);
            btnRepair.gameObject.SetActive(_chooseMode != ChooseMode.InventoryBurn ||
                                           _chooseMode != ChooseMode.InventoryFusion);
        }

        private void FilterHeroesSuitableToUpgrade() {
            // Gỡ ra Hero đang được chọn
            var baseHero = _items.FirstOrDefault(e => e.Item.playerData.heroId == _baseHeroId);
            if (baseHero) {
                _items.Remove(baseHero);
                Destroy(baseHero.gameObject);
            }

            // Gỡ ra Hero khác level
            if (_baseHeroLevel != 0) {
                var diff = _items.Where(e => e.Item.playerData.level != _baseHeroLevel).ToList();
                diff.ForEach(e => {
                    _items.Remove(e);
                    Destroy(e.gameObject);
                });
            }
        }

        public void OnSelectAsMaterial() {
            if (SelectId == default) {
                return;
            }

            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onSelectAsMaterialCallback?.Invoke(SelectId);
            Hide();
        }

        public void OnPrevPageClicked() {
            if (_currentPage <= 0) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _currentPage -= 1;
            inputField.text = $"{_currentPage + 1}";
        }

        public void OnNextPageClicked() {
            if (_currentPage >= _totalPages - 1) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _currentPage += 1;
            inputField.text = $"{_currentPage + 1}";
        }

        private async void OnPageChange(string text) {
            if (!int.TryParse(inputField.text, out var amount)) {
                inputField.text = $"{_currentPage + 1}";
                return;
            }
            var valid = Math.Clamp(amount, 1, _totalPages);
            inputField.text = $"{valid}";
            _currentPage = valid - 1;
            scroller.gameObject.SetActive(false);
            await InstantiateItems(true, _sortRarity, (SortOrder1)dropDown1.value, (SortOrder2)dropDown2.value,
                (HeroTypeFilter)dropDownHeroFilter.value, ConvertFromValue(dropDownActiveFilter.value));
            scroller.gameObject.SetActive(true);

        }

        public ChooseMode GetChooseMode() {
            return _chooseMode;
        }

        public void OpenTouchScreenKeyboard() {
            if (CanShowVirtualKeyboard() && keyboard != null) {
                keyboard.OpenKeyboard(true, false);
                keyboard.OnValueChanged += UpdatePageText;
                keyboard.OnKeyboardClosed += CloseTouchScreenKeyboard;
                if (dialogTransform != null) {
                    dialogTransform.offsetMin = new Vector2(dialogTransform.offsetMin.x, 340);
                }
            }
        }

        private bool CanShowVirtualKeyboard() {
#if UNITY_EDITOR
            return true;
#endif
            return Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform &&
                   AppConfig.IsTon();
        }

        private void CloseTouchScreenKeyboard() {
            if (keyboard != null) {
                inputField.text = keyboard.GetInput();
                keyboard.ClearInput();
                keyboard.OnValueChanged -= UpdatePageText;
                if (dialogTransform != null) {
                    dialogTransform.offsetMin = new Vector2(dialogTransform.offsetMin.x, _originalBottom);
                }
            }
        }

        #endregion

        public enum ChooseMode {
            None,
            Upgrade,
            StoryModePlayToEarn,
            StoryModePlayForFun,
            PvpMode,
            ResetSkill,
            ResetRoi,
            InventoryBurn,
            InventoryFusion,
            PreviewSummary
        }
    }
}