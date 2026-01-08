using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Controller;

using Cysharp.Threading.Tasks;

using Engine.Manager;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogFusionPolygon : Dialog, IDialogFusion {
        [SerializeField]
        private FusionAvatar[] fusionMainAvatars;

        [SerializeField]
        private FusionAvatar fusionSecondAvatar;

        [SerializeField]
        private FusionItemDisplayPolygon fusionItemDisplayPolygon;

        [SerializeField]
        private Text resultFusion;

        [SerializeField]
        private Button fusionBtn;

        private const int MaxLstMainHeroId = 4;
        private int _targetUpgradeRarity = 1; //default rare
        private PlayerData[] _mainLstHeroId;
        private List<PlayerData> _secondLstHeroId;

        private ISoundManager _soundManager;
        private IPlayerStorageManager _playerStoreManager;
        private IBlockchainManager _blockchainManager;
        private IServerManager _serverManager;
        private IStorageManager _storageManager;
        private DialogFusionControllerPolygon _dialogFusionController;

        public static UniTask<DialogFusionPolygon> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogFusionPolygon>();
        }

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _dialogFusionController = new DialogFusionControllerPolygon(_blockchainManager, _serverManager, _storageManager);
            Init();
        }

        private void Init() {
            _mainLstHeroId = new PlayerData[MaxLstMainHeroId];
            _secondLstHeroId = new List<PlayerData>();

            for (var i = 0; i < fusionMainAvatars.Length; i++) {
                fusionMainAvatars[i].Init(i, ChooseMainIdHero, null);
                fusionMainAvatars[i].SetData(null);
            }

            fusionSecondAvatar.Init(0, ChooseSecondIdHero, null);
            fusionSecondAvatar.SetDatas(new List<PlayerData>());
            fusionSecondAvatar.gameObject.SetActive(false);

            fusionItemDisplayPolygon.Init(_targetUpgradeRarity, SetTargetUpgradeRarity);
            fusionBtn.interactable = DialogFusionControllerPolygon.CanFusion(_mainLstHeroId.Where(e => e != null).ToList());
            AddEvent();
        }
        
        private void AddEvent() {
            EventManager<PlayerData>.Add(StakeEvent.AfterStake, ResetUiAfterStake);
        }
        private void RemoveEvent() {
            EventManager<PlayerData>.Remove(StakeEvent.AfterStake, ResetUiAfterStake);
        }

        protected override void OnDestroy() {
            RemoveEvent();
            base.OnDestroy();
        }

        private async void ChooseMainIdHero(int itemIndex) {
            var countSelectHero = _mainLstHeroId.Where(e => e != null).ToList().Count;
            DialogInventory.MaxSelectChooseHero = Mathf.Min(4 - countSelectHero, 4);
            var inventory = await DialogInventoryCreator.Create();
            inventory.Init(DialogInventory.SortRarity.BelowOneLevel, _targetUpgradeRarity, false);
            var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !CanProcessThisHero(e))
                .Select(e => e.heroId).ToArray();
            inventory.SetChooseHeroForInventoryFusion(_mainLstHeroId, exclude, DisplayMainHeroWithId);
            inventory.Show(DialogCanvas);
        }

        private async void ChooseSecondIdHero(int itemIndex) {
            //Cho chọn tối đa 200 hero nguyên liệu phụ đề fusion
            DialogInventory.MaxSelectChooseHero = 200;
            var inventory = await DialogInventoryCreator.Create();
            inventory.Init(DialogInventory.SortRarity.BelowThanOneLevel, _targetUpgradeRarity, false);
            var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !CanProcessThisHero(e))
                .Select(e => e.heroId).ToArray();
            inventory.SetChooseHeroForInventoryFusion(_secondLstHeroId.ToArray(), exclude, DisplaySecondHeroWithId);
            inventory.Show(DialogCanvas);
        }

        private void DisplayMainHeroWithId(PlayerData[] playersData) {
            for (var i = 0; i < Mathf.Min(playersData.Length, fusionMainAvatars.Length); i++) {
                fusionMainAvatars[i].SetData(playersData[i]);
                _mainLstHeroId[i] = playersData[i];
            }

            //Mở nguyên liêu hero buff nếu đã chon dc 3 nguyên liêu chính
            var chooseMainHeroes = _mainLstHeroId.Where(e => e != null).ToArray();
            if (chooseMainHeroes.Length == 3) {
                fusionSecondAvatar.gameObject.SetActive(_targetUpgradeRarity != 1);
            }
            //Đóng ngyên liêu buff và clean nếu chon đủ 4 nguyên liệu chính
            else if (chooseMainHeroes.Length == 4) {
                fusionSecondAvatar.gameObject.SetActive(false);
                CleanSecondDisplay();
            } else {
                fusionSecondAvatar.gameObject.SetActive(false);
            }

            fusionBtn.interactable = DialogFusionControllerPolygon.CanFusion(_mainLstHeroId.Where(e => e != null).ToList());
            ShowUIPercentFusion();
        }

        private void DisplaySecondHeroWithId(PlayerData[] playersData) {
            _secondLstHeroId.Clear();
            foreach (var playerData in playersData) {
                if (playerData != null) {
                    _secondLstHeroId.Add(playerData);
                }
            }
            //Chi add vừa đủ 100%
            RemoveListSecondHeroTarget100Percent();
            fusionSecondAvatar.SetDatas(_secondLstHeroId);
            ShowUIPercentFusion();
        }

        private void SetTargetUpgradeRarity(int targetUpgradeRarity) {
            _targetUpgradeRarity = targetUpgradeRarity;
            CleanMainDisplay();
            CleanSecondDisplay();
        }

        private void ShowUIPercentFusion() {
            resultFusion.text = $"{PercentFusionResult()}%";
        }

        private int PercentFusionResult() {
            var percent = 0f;
            
            // Loại second list khi tính percent nếu second list không xuất hiện.
            var lstHeroId = _mainLstHeroId;
            if (fusionSecondAvatar.gameObject.activeSelf) {
                lstHeroId = _mainLstHeroId.Concat(_secondLstHeroId).ToArray();
            }
            
            foreach (var playerData in lstHeroId) {
                if (playerData != null) {
                    var heroType = _playerStoreManager.GetHeroRarity(playerData);
                    var x = _targetUpgradeRarity - (int) heroType;
                    percent += 25f / Mathf.Pow(4, x - 1);
                }
            }
            return Mathf.RoundToInt(percent);
        }

        private void RemoveListSecondHeroTarget100Percent() {
            var percent = 0f;
            foreach (var playerData in _mainLstHeroId) {
                if (playerData != null) {
                    var heroType = _playerStoreManager.GetHeroRarity(playerData);
                    var x = _targetUpgradeRarity - (int) heroType;
                    percent += 25f / Mathf.Pow(4, x - 1);
                }
            }

            var subSecondHero = new List<PlayerData>();
            foreach (var playerData in _secondLstHeroId) {
                if (playerData != null) {
                    var heroType = _playerStoreManager.GetHeroRarity(playerData);
                    var x = _targetUpgradeRarity - (int) heroType;
                    percent += 25f / Mathf.Pow(4, x - 1);
                    subSecondHero.Add(playerData);
                    if (Mathf.Approximately(percent, 100)) {
                        break;
                    }
                }
            }
            _secondLstHeroId = subSecondHero;
        }

        private void CleanMainDisplay() {
            foreach (var fusionAvatar in fusionMainAvatars) {
                fusionAvatar.SetData(null);
            }
            for (var i = 0; i < _mainLstHeroId.Length; i++) {
                _mainLstHeroId[i] = null;
            }
            ShowUIPercentFusion();
        }
        
        //Reset ui fusion sau khi unstake 1 hero ko còn là S
        private void ResetUiAfterStake(PlayerData playerData) {
            CleanMainDisplay();
            CleanSecondDisplay();
            fusionBtn.interactable = DialogFusionControllerPolygon.CanFusion(_mainLstHeroId.Where(e => e != null).ToList());
        }

        private void CleanSecondDisplay() {
            _secondLstHeroId.Clear();
            fusionSecondAvatar.SetDatas(new List<PlayerData>());
            fusionSecondAvatar.gameObject.SetActive(false);
            ShowUIPercentFusion();
        }

        public async void OnFusionBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (!DialogFusionControllerPolygon.CanFusion(_mainLstHeroId.Where(e => e != null).ToList())) {
                return;
            }
            //Kiểm tra trước xem có hero S fake nào bj burn ko
            var heroFakeCount = CheckStakedBeforeBurn(_mainLstHeroId, _secondLstHeroId);
            if (heroFakeCount > 0) {
                var confirm = await DialogConfirmBurnOrFusion.Create();
                confirm.SetInfo(
                    heroFakeCount,
                    PerformFusion,
                    () => { }
                );
                confirm.Show(DialogCanvas);
                
                return;
            }
            PerformFusion();
        }
        
        /// <summary>
        /// Trả về true nếu trong list có hero S fake
        /// </summary>
        /// <param name="mainList"></param>
        /// <param name="secondList"></param>
        /// <returns></returns>
        private int CheckStakedBeforeBurn(PlayerData[] mainList, List<PlayerData> secondList) {
            var heroFakeCount = 0;
            foreach (var heroId in mainList) {
                var player = heroId == null ? null : _playerStoreManager.GetPlayerDataFromId(heroId.heroId);
                if (player == null)
                    continue;
                
                if (player.HaveAnyStaked()) {
                    heroFakeCount++;
                }
                
            }
            foreach (var heroId in secondList) {
                var player = heroId == null ? null : _playerStoreManager.GetPlayerDataFromId(heroId.heroId);
                if (player == null)
                    continue;
                
                if (player.HaveAnyStaked()) {
                    heroFakeCount++;
                }
                
            }
            
            return heroFakeCount;
        }
        
        private void PerformFusion() {
            fusionBtn.interactable = false;

            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(DialogCanvas);
                waiting.ShowLoadingAnim();
                
                try {
                    var changeWaiting = new Action(() => { waiting.ChangeText("Processing Token Request"); });
                    var fusionFailReasonError = new Action(() => { DialogOK.ShowError(DialogCanvas, "Fusion Failed"); });
                    await _dialogFusionController.Fusion(DialogCanvas, _mainLstHeroId.ToList(), _secondLstHeroId,
                        changeWaiting,
                        fusionFailReasonError);
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    CleanMainDisplay();
                    CleanSecondDisplay();
                    fusionBtn.interactable =
                        DialogFusionControllerPolygon.CanFusion(_mainLstHeroId.Where(e => e != null).ToList());
                    waiting.Hide();
                }
            });
        }

        private static bool CanProcessThisHero(PlayerData hero) {
            if (hero == null || !hero.IsHeroS|| hero.AccountType == HeroAccountType.Trial) {
                return false;
            }
            return true;
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}
