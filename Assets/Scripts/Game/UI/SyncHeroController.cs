using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using Services.Rewards;
using Share.Scripts.Social;
using UnityEngine;

namespace Game.UI {
    public class SyncHeroController : MonoBehaviour {
        public bool StopSyncing { get; set; }
        private HeroId[] _newIds;
        private int _amountNewHero;
        private bool _isSkip, _showSumMary;
        
        private IBHeroManager _bHeroManager;
        
        private Canvas _dialogCanvas;
        private LevelScene _levelScene;
        private ObserverHandle _handle;
        private bool _isBuyHero;

        public void Init(Canvas dialogCanvas, LevelScene levelScene) {
            _dialogCanvas = dialogCanvas;
            _levelScene = levelScene;
        }

        private void Awake() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            
            _handle = new ObserverHandle();
            _isBuyHero = false;
            _handle.AddObserver(serverManager, new ServerObserver {
                    OnNewHeroFi = OnSyncHero,
                    OnNewHeroServer = NewHeroServer,
                });
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        private void OnSyncHero(int[] newHeroIds, bool isBuyHero) {
            _isBuyHero = isBuyHero;
            
            if (StopSyncing) {
                return;
            }
            _isSkip = false;
            _newIds = _bHeroManager
                .GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => newHeroIds.Contains(e.heroId.Id))
                .Select(e => e.heroId)
                .ToArray();

            if (newHeroIds.Length > 0) {
                _amountNewHero = newHeroIds.Length;
                _showSumMary = _amountNewHero > 1;
                ShowNewHero(0, _amountNewHero == 1);
            }
        }

        private void NewHeroServer(int[] newHeroIds, bool isBuyHero) {
            _isBuyHero = isBuyHero;
            
            _isSkip = false;
            //DevHoang: Add new airdrop
            HeroAccountType heroType = HeroAccountType.Nft;
            if (AppConfig.IsTon()) {
                heroType = HeroAccountType.Ton;
            } else if (AppConfig.IsSolana()) {
                heroType = HeroAccountType.Sol;
            } else if (AppConfig.IsRonin()) {
                heroType = HeroAccountType.Ron;
            } else if (AppConfig.IsBase()) {
                heroType = HeroAccountType.Bas;
            } else if (AppConfig.IsViction()) {
                heroType = HeroAccountType.Vic;
            }
            _newIds = _bHeroManager
                .GetPlayerDataList(heroType)
                .Where(e => newHeroIds.Contains(e.heroId.Id))
                .Select(e => e.heroId)
                .ToArray();
            if (newHeroIds.Length > 0) {
                _amountNewHero = newHeroIds.Length;
                _showSumMary = _amountNewHero > 1;
                ShowNewHero(0, _amountNewHero == 1);
            }
        }
        
        private async void ShowNewHero(int index, bool isBuyOne = false) {
            void ShowNextHero(int currentIndex) {
                if (currentIndex + 1 < _newIds.Length && !_isSkip) {
                    ShowNewHero(currentIndex + 1);
                } else {
                    if (_levelScene != null) {
                        AddPlayers();
                    }
                }
            }
            
            PlayerData newHero = null;
            if (index < _newIds.Length) {
                newHero = _bHeroManager.GetPlayerDataFromId(_newIds[index]);
            }
            if (newHero == null) {
                ShowNextHero(index);
            } else {
                _amountNewHero--;
                
                var dialog = await DialogNewHero.Create();
                
                // điều kiện dừng đệ quy
                if (_amountNewHero < 1 && !isBuyOne) {
                    dialog.OnDidHide(SkipHero);
                    dialog.Show(_dialogCanvas);
                    dialog.SetInfo(newHero, SkipHero, _amountNewHero);
                    ConfigShareButton(dialog, "new_hero", new[] { newHero });
                    return;
                }

                dialog.OnDidHide(() => ShowNextHero(index));
                dialog.Show(_dialogCanvas);
                dialog.SetInfo(newHero, SkipHero, _amountNewHero);
                ConfigShareButton(dialog, "new_hero", new[] { newHero });
            }
        }
        
        private void SkipHero() {
            if (_levelScene != null) {
                AddPlayers();
            }
            ShowSummary();
            _isSkip = true;
        }
        
        private async void ShowSummary() {
            if (_showSumMary) {
                //Hiện lại dialog mua hero
                var dialog = await DialogInventoryCreator.Create();
                dialog.SetOnHideBySelf(() => {
                    UniTask.Void(async () => {
                        //Nếu dialog tổng kết này ko phải là do buy hero thì ko hiện lại dialog mua hero
                        if(!_isBuyHero)
                            return;
                        _isBuyHero = false;
                        if (AppConfig.IsTon()) {
                            var shop = await DialogShopHeroTon.Create();
                            shop.Init();
                            shop.Show(_dialogCanvas);
                        }
                        else if (AppConfig.IsSolana()) {
                            var shop = await DialogShopHeroSolana.Create();
                            shop.Init();
                            shop.Show(_dialogCanvas);
                        }
                        else if (AppConfig.IsWebAirdrop()) {
                            var shop = await DialogShopHeroWebAirdrop.Create();
                            shop.Init();
                            shop.Show(_dialogCanvas);
                        }
                        else {
                            var shop = await DialogShopHero.Create();
                            shop.Init(true);
                            shop.Show(_dialogCanvas);
                        }
                    });
                });
                //Bỏ các tính năng ko liên quan, chỉ show hero cho user xem
                dialog.SetChooseHeroForPreviewSummary();
                var listNewHero = _bHeroManager.GetPlayerDataList(_newIds);
                if (AppConfig.IsAirDrop()) {
                    dialog.Show(_dialogCanvas, () => listNewHero, "ALL PURCHASED HEROES");
                } else {
                    dialog.Show(_dialogCanvas, () => listNewHero, "ALL MINTED HEROES");
                }
                ConfigShareButton(dialog as Game.Dialog.Dialog, "summary", listNewHero);
            }
        }

        // Nút Share on X được designer đặt sẵn trong prefab dialog. Bơm context (source + danh
        // sách hero) cho TẤT CẢ nút trong dialog — DialogNewHero có 2 nút (1 cạnh btnSkip, 1 cạnh
        // btnContinue), chỉ 1 hiện theo amount; nếu chỉ config nút đầu thì nút còn lại _payload null
        // -> NRE khi click. Vòng đời nút = theo dialog (tự sinh/huỷ).
        private void ConfigShareButton(Component dialogRoot, string source, IReadOnlyList<PlayerData> heroes) {
            if (!dialogRoot) {
                return;
            }
            var buttons = dialogRoot.GetComponentsInChildren<ShareOnXButton>(true);
            if (buttons.Length == 0) {
                return;
            }
            var payload = SharePayload.ForHeroes(source, heroes);
            foreach (var button in buttons) {
                button.SetContext(_dialogCanvas, payload);
            }
        }

#if UNITY_EDITOR
        // Share on X — debug test (tạm, gỡ sau khi xong): chạy trong FarmingScene Play mode,
        // chọn GameObject chứa SyncHeroController -> context menu component -> chọn entry.
        // Mở dialog mẫu RỒI gắn nút Share (giống flow thật) để test capture/preview.

        [ContextMenu("DEBUG: Show Share Summary")]
        private async void DebugShowShareSummary() {
            if (!_dialogCanvas) {
                Debug.LogError("[ShareDebug] _dialogCanvas chưa Init — chạy trong FarmingScene Play mode.");
                return;
            }
            var heroes = DebugSampleHeroes(50);
            if (heroes.Count == 0) {
                Debug.LogError("[ShareDebug] không tìm thấy hero mẫu nào.");
                return;
            }
            var dialog = await DialogInventoryCreator.Create();
            dialog.SetChooseHeroForPreviewSummary();
            dialog.Show(_dialogCanvas, () => heroes, "DEBUG SHARE SUMMARY");
            Debug.Log($"[ShareDebug] summary với {heroes.Count} hero mẫu.");
            ConfigShareButton(dialog as Game.Dialog.Dialog, "summary", heroes);
        }

        [ContextMenu("DEBUG: Show Share New Hero")]
        private async void DebugShowShareNewHero() {
            if (!_dialogCanvas) {
                Debug.LogError("[ShareDebug] _dialogCanvas chưa Init — chạy trong FarmingScene Play mode.");
                return;
            }
            var hero = DebugSampleHeroes(1).FirstOrDefault();
            if (hero == null) {
                Debug.LogError("[ShareDebug] không tìm thấy hero mẫu nào.");
                return;
            }
            var dialog = await DialogNewHero.Create();
            dialog.Show(_dialogCanvas);
            dialog.SetInfo(hero, null, 0);
            Debug.Log($"[ShareDebug] new hero mẫu id={hero.heroId.Id}.");
            ConfigShareButton(dialog, "new_hero", new[] { hero });
        }

        [ContextMenu("DEBUG: Show Share Token Reward")]
        private async void DebugShowShareTokenReward() {
            if (!_dialogCanvas) {
                Debug.LogError("[ShareDebug] _dialogCanvas chưa Init — chạy trong FarmingScene Play mode.");
                return;
            }
            // Mock token: chỉ cần displayName + amount để test capture/payload (icon null -> ô icon trống).
            var token = new TokenData { displayName = "BCOIN", tokenName = BlockRewardType.BCoin };
            const double amount = 1234.56;
            var dialog = await DialogBCoinReward.Create();
            // SetReward tự bơm context cho nút Share (phương án B) -> không cần gọi ConfigShareButton.
            dialog.SetReward(token, amount, _dialogCanvas).Show(_dialogCanvas);
            Debug.Log($"[ShareDebug] token reward mẫu {amount} {token.displayName}.");
        }

        private List<PlayerData> DebugSampleHeroes(int count) {
            return _bHeroManager
                .GetPlayerDataList(HeroAccountType.Nft, HeroAccountType.Ton, HeroAccountType.Sol,
                    HeroAccountType.Ron, HeroAccountType.Bas, HeroAccountType.Vic)
                .Take(count)
                .ToList();
        }
#endif

        
        private void AddPlayers() {
            if (_newIds.Length > 0) {
                if (_levelScene) {
                    _levelScene.AddNewPlayersOrRefresh(_newIds);
                }
            }
        }

        private async void ShowHeroWithIndexOn(int index) {
            var dialog = await DialogHeroesCreator.Create();
            dialog.OnDidShow(() => { dialog.SelectItem(index); });
            dialog.Show(_dialogCanvas);
        }

        private async void ShowHeroInventory() {
            var dialog = await DialogInventoryCreator.Create();
            dialog.Show(_dialogCanvas);
        }
    }
}