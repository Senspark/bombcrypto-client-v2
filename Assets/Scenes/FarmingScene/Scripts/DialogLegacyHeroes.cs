using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Engine.Manager;
using Game.Dialog;
using Game.Manager;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    
    public class DataUnStake {
        public PlayerData PlayerData;
        public string Staked;
        public string UnStake;
        public string Fee;
        public string TotalUnStake;
        public string StakeRemain;
        public string MinValue;
        public RewardType TokenType;
    }
    
    public class DialogLegacyHeroes : Dialog {
        [SerializeField]
        private ScrollRect scroller;

        [SerializeField]
        private InventoryItem inventoryItemPrefab;

        [SerializeField]
        private HeroDetailsDisplay heroDescriptionPanel;

        [SerializeField]
        private Dropdown dropDown1;

        [SerializeField]
        private Dropdown dropDown2;

        [SerializeField]
        private Button buttonLegacy;

        [SerializeField]
        public GameObject pageContent;
        
        [SerializeField]
        private TMP_InputField inputField;

        [SerializeField]
        private TMP_Text numPages;
        
        public delegate List<PlayerData> GetPlayer();

        public float ScrollValue { get; set; } = -1;
        public HeroId SelectId { get; set; } = default;
        public Vector2Int ColumnRow { get; set; }
        public DialogInventory.SortOrder1 Order1 { get; set; } = DialogInventory.SortOrder1.ActiveFirst;
        public DialogInventory.SortOrder2 Order2 { get; set; } = DialogInventory.SortOrder2.HighStatsFirst;
        public DialogInventory.HeroTypeFilter Filter1 { get; set; } = DialogInventory.HeroTypeFilter.AllHeroesType;

        private IServerManager _serverManager;
        private IPlayerStorageManager _playerStore;
        private ISoundManager _soundManager;

        private List<InventoryItem> _items;
        private DialogInventory.ChooseMode _chooseMode = DialogInventory.ChooseMode.None;
        private Sequence _timeOutTween;
        private WaitingUiManager _waiting;
        private CancellationTokenSource _uiTaskCancellation;
        private ObserverHandle _handle;
        private List<PlayerData> _heroesIdBurn = new();
        private DialogInventory.SortRarity _sortRarity = DialogInventory.SortRarity.Default;
        private int _targetUpgradeRarity; // Fusion hero
        public static int MaxSelectChooseHero;

        private Action<int[]> _callBackBurnHeroId;
        private GetPlayer _getPlayer;
        private bool _notHover;
        private PlayerData _currentHeroIdSelected;
        private InventoryItem _currentInventoryItem;

        private const int PageRow = 12;
        private int _itemsInPage = 50;
        private int _currentPage;
        private int _totalPages;
        private List<int> _totalHeroIds = new List<int>();
        private TouchScreenKeyboard _keyboard;

        public static UniTask<DialogLegacyHeroes> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLegacyHeroes>();
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _uiTaskCancellation = new CancellationTokenSource();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnSyncHero = OnSyncHero
            });
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var enableRepair = featureManager.EnableRepairShield;
            var showGroupButton = true;
            var enableOpenStake = false;
            heroDescriptionPanel.Init(enableRepair, showGroupButton, enableOpenStake);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            EndWait();
            _uiTaskCancellation.Cancel();
            _uiTaskCancellation.Dispose();
            _handle.Dispose();
        }

        public override void Show(Canvas canvas) {
            base.Show(canvas);
            ScrollValue = -1;
            pageContent.SetActive(false);
            scroller.gameObject.SetActive(false);
            
            // mặc định không load player list và sort
            // mà get trực tiếp sorted player data.
            // _getPlayer ??= () => _playerStore.GetPlayerDataList(HeroAccountType.Nft, HeroAccountType.Trial);

            BeginWait();
            heroDescriptionPanel.HideInfo();
            SetDataToDropDown();
            
            _currentPage = 0;
            inputField.text = $"{_currentPage + 1}";
            UniTask.Void(async () => {
                // Delay 1 frame để lấy items in a page (bội số của column trong grid layout)
                await UniTask.DelayFrame(1);
                await InstantiateItems(true, _sortRarity, Order1, Order2);
                scroller.gameObject.SetActive(true);
                EndWait();
            });

            inputField.onValueChanged.AddListener(OnPageChange);
            inputField.onSelect.AddListener(OpenTouchScreenKeyboard);
            inputField.onDeselect.AddListener(CloseTouchScreenKeyboard);
        }

        public void Show(Canvas canvas, GetPlayer getPlayer) {
            _getPlayer = getPlayer;
            Show(canvas);
        }

        protected override void OnYesClick() {
            // Do nothing
        }

        #region PUBLIC METHODS

        public void Init(DialogInventory.SortRarity sortRarity, int targetUpgradeRarity, bool enableRepair) {
            _targetUpgradeRarity = targetUpgradeRarity;
            _sortRarity = sortRarity;
            heroDescriptionPanel.Init(enableRepair);
        }

        public async UniTask InstantiateItems(
            bool scroll,
            DialogInventory.SortRarity rarity,
            DialogInventory.SortOrder1 order1 = DialogInventory.SortOrder1.ActiveFirst,
            DialogInventory.SortOrder2 order2 = DialogInventory.SortOrder2.HighStatsFirst,
            DialogInventory.HeroTypeFilter filter1 = DialogInventory.HeroTypeFilter.AllHeroesType,
            DialogInventory.ActiveFilter filterActive = DialogInventory.ActiveFilter.All) {
            var container = scroller.content.transform;
            foreach (Transform child in container) {
                Destroy(child.gameObject);
            }

            // Tính số column của grid theo chiểu ngang của view port 
            var gridLayoutGroup = scroller.content.GetComponent<GridLayoutGroup>();
            var rect = scroller.viewport.rect;
            var width = rect.width;
            var itemWidth = gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x;
            var column = (int) (width / itemWidth);
            _itemsInPage = column * PageRow;
            
            List<PlayerData> players;
            if (_getPlayer == null) {
                // Get sorted data có ExcludeHeroIds trước khi Phân trang 
                players = GetSortedPlayerData(rarity, _targetUpgradeRarity, order1, order2, filter1, filterActive);
            } else {
                
                // Trường hợp _getPlayer != null là
                // trường hợp hiển thị danh sách new heroes (tối đa 15) => không phân trang.
                players = SortPlayerData(_getPlayer(), rarity, _targetUpgradeRarity, order1, order2,
                    filter1);
                pageContent.SetActive(false);
            }            

            var num = players.Count;
            buttonLegacy.interactable = num > 0;
            _items = new List<InventoryItem>();

            var inventoryItemCallback = new InventoryItem.InventoryItemCallback();
            if (_chooseMode == DialogInventory.ChooseMode.InventoryBurn) {
                inventoryItemCallback.OnClicked = OnItemClickedInventoryBurn;
            } else if (_chooseMode == DialogInventory.ChooseMode.InventoryFusion) {
                inventoryItemCallback.OnClicked = OnItemClickedInventoryFusion;
            } else {
                inventoryItemCallback.OnClicked = OnItemClicked;
            }
            inventoryItemCallback.OnHover = OnItemHover;

            for (var i = 0; i < num; i++) {
                if (players[i] != null) {
                    var item = Instantiate(inventoryItemPrefab, container, false);
                    _heroesIdBurn = await item.SetInfo(players[i], inventoryItemCallback, _chooseMode, _heroesIdBurn,
                        _chooseMode == DialogInventory.ChooseMode.PvpMode,
                        _heroesIdBurn.Contains(players[i]));
                    _items.Add(item);
                }
            }

            if (_chooseMode == DialogInventory.ChooseMode.Upgrade) {
                FilterHeroesSuitableToUpgrade();
            }

            // ExcludeHeroes sẽ thực hiện trong phần GetSortedData
            // (exclude trước khi phân trang)
            //FilterExcludeHeroes();

            StartCoroutine(GetRowAndColumn());
            StartCoroutine(ScrollToCoroutine(scroll));
            
            // Tự chọn hero đầu tiên trong ds
            if (_items.Count > 0) {
                OnItemClicked(_items[0]);
            }
        }

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
            if (itemCount == 0 || ColumnRow.x == 0 || ColumnRow.y == 0) {
                return;
            }

            itemIndex = Mathf.Clamp(itemIndex, 0, itemCount - 1);
            var row = (itemIndex) / ColumnRow.x;
            var normal = (float)row / (ColumnRow.y - 1);
            scroller.verticalNormalizedPosition = 1f - normal;
        }

        #endregion

        #region EVENT METHODS

        private void OnItemClicked(InventoryItem item) {
            UpdateItemInfo(item);
            SetHighLight(item);
            _notHover = true;
        }

        private void SetHighLight(InventoryItem item) {
            foreach (var iter in _items) {
                iter.SetHighLight(iter == item);
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
                --MaxSelectChooseHero;
                //Add giá trị vào đầu list
                for (var i = 0; i < _heroesIdBurn.Count; i++) {
                    if (_heroesIdBurn[i] == null) {
                        _heroesIdBurn[i] = playerData;
                        return;
                    }
                }
                _heroesIdBurn.Add(playerData);
            } else {
                ++MaxSelectChooseHero;
                //Add giá trị vào đầu list
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

            _currentHeroIdSelected = item.playerData;
            _currentInventoryItem = item;
        }

        public void UpdateHeroInfo(PlayerData player) {
            if (_currentInventoryItem == null)
                return;
            var index = _items.IndexOf(_currentInventoryItem);
            if (index < 0)
                return;
            _items[index].UpdateInfo(player);
        }

        public void OnActiveClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);

            if (_playerStore.GetActivePlayersAmount() >= 15) {
                var str = ServiceLocator.Instance.Resolve<ILanguageManager>().GetValue(LocalizeKey.err_hero_max_active);
                DialogOK.ShowError(DialogCanvas, str);
                return;
            }

            BeginWait();
            UniTask.Void(async () => {
                HideButtonsAndPanels();
                await _serverManager.Pve.ActiveBomber(SelectId, 1);
                if (_uiTaskCancellation.IsCancellationRequested) {
                    return;
                }
                SortInventory();
                EndWait();
            });
        }

        public void OnDeactiveClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            BeginWait();
            UniTask.Void(async () => {
                HideButtonsAndPanels();
                await _serverManager.Pve.ActiveBomber(SelectId, 0);
                if (_uiTaskCancellation.IsCancellationRequested) {
                    return;
                }
                SortInventory();
                EndWait();
            });
        }

        private void OnSyncHero(ISyncHeroResponse _) {
            var scrollValue = Mathf.Clamp01(scroller.verticalNormalizedPosition);
            var sortOrder1 = (DialogInventory.SortOrder1)dropDown1.value;
            var sortOrder2 = (DialogInventory.SortOrder2)dropDown2.value;
            var filter1 = DialogInventory.HeroTypeFilter.HeroOnly;
            InstantiateItems(true, _sortRarity, sortOrder1, sortOrder2, filter1);
            ScrollValue = scrollValue;
        }

        public void OnCloseBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnHide();
        }

        private void OnHide() {
            Hide();
        }

        public void OnBtnLegacy() {
            _soundManager.PlaySound(Audio.Tap);

            if (_currentHeroIdSelected.Shield != null && !_currentHeroIdSelected.IsHeroS) {
                //popupStakeHeroS.Show(_currentHeroIdSelected);
                DialogStakeHeroesPlus.Create().ContinueWith(dialog => {
                    dialog.Show(_currentHeroIdSelected, DialogCanvas, GetCallBack());    
                });
            } else {
                //popupStakeLegacy.Show(_currentHeroIdSelected);
                DialogStakeHeroesS.Create().ContinueWith(dialog => {
                    dialog.Show(_currentHeroIdSelected, DialogCanvas, GetCallBack());    
                });
            }
        }

        private StakeCallback.Callback GetCallBack() {
            var callback = new StakeCallback()
                .OnStakeComplete(player => {
                    EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
                    _currentHeroIdSelected = player;
                })
                .OnUnStakeComplete(player => {
                    EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
                    _currentHeroIdSelected = player;
                })
                .Create();
            
            return callback;
        }

        #endregion

        #region PRIVATE METHODS

        private void HideButtonsAndPanels() {
            heroDescriptionPanel.HideInfo();
        }

        private IEnumerator ScrollToCoroutine(bool scroll) {
            if (_items.Count == 0) {
                yield break;
            }

            yield return null;
            yield return null;

            //var index = _items.FindIndex(e => e.playerData.heroId == SelectId);
            var index = 0;
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

        private IEnumerator GetRowAndColumn() {
            yield return null;
            var grid = scroller.content.GetComponent<GridLayoutGroup>();
            ColumnRow = GridLayoutGroupUtil.GetColumnAndRow(grid);
        }

        #endregion

        #region SORTING

        #region Player Data Sort

        private void SetDataToDropDown() {
            dropDown1.options.Clear();
            dropDown2.options.Clear();

            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();

            dropDown1.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_active)));
            dropDown1.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_unactive)));

            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_high_stats)));
            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_high_rarity)));
            dropDown2.options.Add(new Dropdown.OptionData(languageManager.GetValue(LocalizeKey.ui_newest)));

            //Update text dropdown
            dropDown1.captionText.text = dropDown1.options[0].text;
            dropDown2.captionText.text = dropDown2.options[0].text;
            
            dropDown1.value = (int)Order1;
            dropDown2.value = (int)Order2;

            dropDown1.onValueChanged.AddListener(_ => SortInventory());
            dropDown2.onValueChanged.AddListener(_ => SortInventory());
        }

        private void SortInventory() {
            InstantiateItems(false, _sortRarity, (DialogInventory.SortOrder1)dropDown1.value, (DialogInventory.SortOrder2)dropDown2.value);
        }

        private List<PlayerData> GetSortedPlayerData(
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder1 order1,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3,
            DialogInventory.ActiveFilter filterActive
            ) {

            var (list, totalPages, totalHeroIds) = _playerStore.GetSortedPlayerDataList(orderRarity, targetRarity,
                order1, order2, order3, filterActive,
                null,
                _excludeHeroIds,
                _currentPage, _itemsInPage,
                HeroAccountType.Nft, HeroAccountType.Ton,HeroAccountType.Trial, HeroAccountType.Sol
            );
            
            _totalPages = totalPages;
            _totalHeroIds = totalHeroIds;
            numPages.text = $"/{_totalPages}";
            pageContent.SetActive(_totalPages > 1);
            
            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(DefaultPlayerStoreManager.GeneratePlayerData(t));
            }
            return result;
        }            
        
        private static List<PlayerData> SortPlayerData(
            List<PlayerData> input,
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder1 order1,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3) {
            input = orderRarity switch {
                DialogInventory.SortRarity.Default => input,
                DialogInventory.SortRarity.BelowOneLevel => input.Where(e => e.rare == targetRarity).ToList(),
                DialogInventory.SortRarity.BelowThanOneLevel => input.Where(e => e.rare < targetRarity).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(orderRarity), orderRarity, null)
            };
            //_playerStoreManager.GetHeroAbility(e) =="";
            var result1 = order3 switch {
                DialogInventory.HeroTypeFilter.AllHeroesType => input,
                DialogInventory.HeroTypeFilter.HeroOnly => input.Where(e => !e.IsHeroS || (!e.IsHeroS && e.Shield != null)),
                DialogInventory.HeroTypeFilter.HeroSOnly => input.Where(e => e.IsHeroS),
                _ => throw new ArgumentOutOfRangeException(nameof(order3), order3, null),
            };

            var result2 = order1 switch {
                DialogInventory.SortOrder1.ActiveFirst => result1.OrderByDescending(e => e.active),
                DialogInventory.SortOrder1.UnActiveFirst => result1.OrderByDescending(e => !e.active),
                _ => throw new ArgumentOutOfRangeException(nameof(order1), order1, null)
            };

            switch (order2) {
                case DialogInventory.SortOrder2.HighStatsFirst:
                    result2 = result2.ThenByDescending(
                        e => e.bombDamage + e.speed + e.stamina + e.bombNum + e.bombRange);
                    break;
                case DialogInventory.SortOrder2.HighRarityFirst:
                    result2 = result2.ThenByDescending(e => e.rare);
                    break;
                case DialogInventory.SortOrder2.NewestFirst:
                    result2 = result2.ThenByDescending(e => e.heroId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order2), order2, null);
            }

            return result2.ToList();
        }

        #endregion

        #endregion

        #region FOR UPGRADE

        private Action<HeroId> _onSelectAsMaterialCallback;
        private HeroId _baseHeroId;
        private int _baseHeroLevel;
        private HeroId[] _excludeHeroIds;

        private void FilterHeroesSuitableToUpgrade() {
            // Gỡ ra Hero đang được chọn
            var baseHero = _items.FirstOrDefault(e => e.playerData.heroId == _baseHeroId);
            if (baseHero) {
                _items.Remove(baseHero);
                Destroy(baseHero.gameObject);
            }

            // Gỡ ra Hero khác level
            if (_baseHeroLevel != 0) {
                var diff = _items.Where(e => e.playerData.level != _baseHeroLevel).ToList();
                diff.ForEach(e => {
                    _items.Remove(e);
                    Destroy(e.gameObject);
                });
            }
        }

        // private void FilterExcludeHeroes() {
        //     if (_excludeHeroIds == null) {
        //         return;
        //     }
        //
        //     foreach (var id in _excludeHeroIds) {
        //         var item = _items.FirstOrDefault(e => e.playerData.heroId == id);
        //         if (item) {
        //             _items.Remove(item);
        //             Destroy(item.gameObject);
        //         }
        //     }
        // }

        public void OnSelectAsMaterial() {
            if (SelectId == default) {
                return;
            }

            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onSelectAsMaterialCallback?.Invoke(SelectId);
            OnHide();
        }
        
        public void OnPrevPageClicked() {
            if (_currentPage <= 0) {
                return;
            }
            _currentPage -= 1;
            inputField.text = $"{_currentPage + 1}";
        }

        public void OnNextPageClicked() {
            if (_currentPage >= _totalPages - 1) {
                return;
            }
            _currentPage += 1;
            inputField.text = $"{_currentPage + 1}";
        }

        private void OnPageChange(string text) {
            if (!int.TryParse(inputField.text, out var amount)) {
                inputField.text = $"{_currentPage + 1}";
                return;
            }
            var valid = Math.Clamp(amount, 1, _totalPages);
            inputField.text = $"{valid}";
            _currentPage = valid - 1;

            InstantiateItems(true, _sortRarity, 
                (DialogInventory.SortOrder1) dropDown1.value, (DialogInventory.SortOrder2) dropDown2.value);
        }
        
        private void OpenTouchScreenKeyboard(string text) {
            if (CanShowVirtualKeyboard()) {
                _keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.NumberPad);
            }
        }
        private bool CanShowVirtualKeyboard() {
            return Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform && AppConfig.IsTon();
        }

        private void CloseTouchScreenKeyboard(string text) {
            if (_keyboard != null) {
                inputField.text = _keyboard.text;
                _keyboard.active = false;
                _keyboard = null;
            }
        }

        #endregion
    }
}