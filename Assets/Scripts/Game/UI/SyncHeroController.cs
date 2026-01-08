using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using UnityEngine;

namespace Game.UI {
    public class SyncHeroController : MonoBehaviour {
        public bool StopSyncing { get; set; }
        private HeroId[] _newIds;
        private int _amountNewHero;
        private bool _isSkip, _showSumMary;
        
        private IPlayerStorageManager _playerStorageManager;
        
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
            _playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            
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
            _newIds = _playerStorageManager
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
            _newIds = _playerStorageManager
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
                newHero = _playerStorageManager.GetPlayerDataFromId(_newIds[index]);
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
                    return;
                }
                
                dialog.OnDidHide(() => ShowNextHero(index));
                dialog.Show(_dialogCanvas);
                dialog.SetInfo(newHero, SkipHero, _amountNewHero);
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
                var listNewHero = _playerStorageManager.GetPlayerDataList(_newIds);
                if (AppConfig.IsAirDrop()) {
                    dialog.Show(_dialogCanvas, () => listNewHero, "ALL PURCHASED HEROES");
                } else {
                    dialog.Show(_dialogCanvas, () => listNewHero, "ALL MINTED HEROES");
                }
            }
        }
        
        
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