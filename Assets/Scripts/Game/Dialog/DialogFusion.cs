using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Scenes.FarmingScene.Scripts;

using Senspark;

using Share.Scripts.Dialog;

using Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogFusion : Dialog, IDialogFusion {
        public enum FusionMode {
            Rarity,
            Level
        }

        [SerializeField]
        private List<FusionItemDisplay> fusionItemDisplays;

        [SerializeField]
        private List<FusionAvatar> fusionAvatars;

        [SerializeField]
        private List<Image> fusionStars;

        [SerializeField]
        private Color defaultStarColor;

        [SerializeField]
        private Color[] rarityColor;

        [SerializeField]
        private List<FusionPercentDisplay> fusionPercentDisplays;

        [SerializeField]
        private HeroDetailsDisplay heroDetailsDisplay;

        [SerializeField]
        private List<GameObject> heroSObjects;

        [SerializeField]
        private Button fusionBtn;

        [SerializeField]
        private Text titleText;

        private const int MAX_HEROES_AMOUNT = 5;
        private int _neededHeroesAmount = 5;
        private const int PERCENT = 20;
        private PlayerData[] _selectedHeroes;
        private int _waitingItemIndex = -1;
        
        private FusionMode _mode = FusionMode.Rarity;
        private PlayerData _baseHero;
        
        private ISoundManager _soundManager;
        private IPlayerStorageManager _playerStoreManager;
        private IBlockchainManager _blockchainManager;
        private IServerManager _serverManager;
        private IFeatureManager _featureManager;
        private IStorageManager _storageManager;
        
        public static DialogFusion Create() {
            var prefab = Resources.Load<DialogFusion>("Prefabs/Dialog/DialogFusion");
            return Instantiate(prefab);
        }

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>(); 
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>(); 
            heroDetailsDisplay.Init(_featureManager.EnableRepairShield);

            if (_selectedHeroes == null) {
                Init(FusionMode.Rarity);
            }
            RenderItemsPercents();
        }

        public void Init(FusionMode mode, PlayerData baseHero = null) {
            _mode = mode;
            _baseHero = baseHero;

            if (_mode == FusionMode.Level && _baseHero != null) {
                // Evolution rules:
                // L1-L5 -> 2 material heroes (Total 3, so index 0 is base, 1-2 are mats)
                // L6-L10 -> 3 material heroes (Total 4, so index 0 is base, 1-3 are mats)
                _neededHeroesAmount = _baseHero.level < 5 ? 3 : 4;
                if (titleText) titleText.text = "HERO EVOLUTION";
            } else {
                _neededHeroesAmount = 5;
                if (titleText) titleText.text = "HERO FUSION";
            }

            _selectedHeroes = new PlayerData[MAX_HEROES_AMOUNT];
            
            for (var i = 0; i < MAX_HEROES_AMOUNT; i++) {
                bool isSlotActive = i < _neededHeroesAmount;
                fusionItemDisplays[i].gameObject.SetActive(isSlotActive);
                fusionAvatars[i].gameObject.SetActive(isSlotActive);

                if (isSlotActive) {
                    fusionItemDisplays[i].Init(i, ChooseHero);
                    fusionItemDisplays[i].SetData(null);
                    fusionAvatars[i].Init(i, ChooseHero, null);
                    fusionAvatars[i].SetData(null);
                }
            }

            if (_mode == FusionMode.Level && _baseHero != null) {
                // Pre-set base hero in the first slot
                _selectedHeroes[0] = _baseHero;
                fusionAvatars[0].SetData(_baseHero);
                fusionItemDisplays[0].SetData(_baseHero);
                // Disable clicking on the base hero slot in evolution mode? 
                // Actually better to let them see it.
            }

            heroDetailsDisplay.Hide();
            fusionBtn.interactable = false;
        }

        public void OnFusionBtnClicked() {
            if (!CanFusion()) {
                return;
            }
            fusionBtn.interactable = false;
            _soundManager.PlaySound(Audio.Tap);
            
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            waiting.ChangeText("Processing...");

            UniTask.Void(async () => {
                try {
                    bool result;
                    if (_mode == FusionMode.Level) {
                        int baseId = _selectedHeroes[0].heroId.Id;
                        int[] materialIds = _selectedHeroes.Skip(1).Take(_neededHeroesAmount - 1).Select(e => e.heroId.Id).ToArray();
                        result = await _blockchainManager.UpgradeHero(baseId, materialIds);
                    } else {
                        var heroIds = _selectedHeroes.Take(MAX_HEROES_AMOUNT).Select(e => e.heroId.Id).ToArray();
                        result = await _blockchainManager.FusionHero(heroIds);
                    }

                    if (result) {
                        waiting.ChangeText("Processing Token Request");
                        result = await ProcessTokenHelper.ProcessTokenRequest(DialogCanvas, _blockchainManager,
                            _serverManager, true);
                    }

                    if (result) {
                        Hide();
                    } else {
                        DialogOK.ShowError(DialogCanvas, "Failed");
                        fusionBtn.interactable = CanFusion();
                    }
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                }
                waiting.End();
            });
        }

        public void OnUnChooseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            heroDetailsDisplay.Hide();
            
            if (_waitingItemIndex < 0 || _waitingItemIndex >= _neededHeroesAmount) {
                return;
            }

            // In Evolution mode, slot 0 is the base hero and cannot be removed
            if (_mode == FusionMode.Level && _waitingItemIndex == 0) {
                return;
            }

            var index = _waitingItemIndex;
            var selectedHero = _selectedHeroes[index]; 
            _waitingItemIndex = -1;

            if (selectedHero == null) {
                return;
            }

            _selectedHeroes[index] = null;
            fusionAvatars[index].SetData(null);
            fusionItemDisplays[index].SetData(null);
            RenderItemsPercents();
        }

        private void DisplayHeroWithId(HeroId heroId) {
            var playerData = _playerStoreManager.GetPlayerDataFromId(heroId);
            if (_waitingItemIndex < 0 || _waitingItemIndex >= _neededHeroesAmount || playerData == null) {
                return;
            }

            if (_featureManager.WarningHeroS && playerData.IsHeroS) {
                DialogOK.ShowInfo(DialogCanvas, "Warning", "You are selecting a BHero S");
            }

            var index = _waitingItemIndex;
            _waitingItemIndex = -1;

            fusionAvatars[index].SetData(playerData);
            fusionItemDisplays[index].SetData(playerData);

            heroDetailsDisplay.SetInfo(playerData, DialogCanvas);
            heroDetailsDisplay.Show();

            _selectedHeroes[index] = playerData;

            SelectItem(index);
            fusionBtn.interactable = CanFusion();
            RenderItemsPercents();
        }

        private async void ChooseHero(int itemIndex) {
            // In Evolution mode, slot 0 is the base hero and cannot be changed
            if (_mode == FusionMode.Level && itemIndex == 0) {
                SelectItem(itemIndex);
                heroDetailsDisplay.SetInfo(_selectedHeroes[0], DialogCanvas);
                heroDetailsDisplay.Show();
                return;
            }

            _waitingItemIndex = itemIndex;
            var selectedHero = _selectedHeroes[itemIndex]; 

            if (selectedHero == null) { 
                // Choose new hero
                var inventory = await DialogInventoryCreator.Create();
                var exclude = _selectedHeroes.Where(e => e != null).Select(e => e.heroId).ToArray();
                
                if (_mode == FusionMode.Level) {
                    // Selection for Evolution: must be same rarity and same level as base hero
                    inventory.SetChooseHeroForUpgrade(_baseHero.heroId, _baseHero.level, heroIds => DisplayHeroesWithIds(heroIds));
                } else {
                    inventory.SetChooseHeroForResetRoi(exclude, DisplayHeroWithId);
                }
                inventory.Show(DialogCanvas);
            } else {
                // display only
                SelectItem(itemIndex);
                heroDetailsDisplay.SetInfo(selectedHero, DialogCanvas);
                heroDetailsDisplay.Show();
            }
        }

        private void DisplayHeroesWithIds(HeroId[] heroIds) {
            if (heroIds == null || heroIds.Length == 0) return;
            
            // Slots for materials start from index 1. 
            // base hero is always at slot 0.
            for (int i = 0; i < heroIds.Length; i++) {
                int slotIndex = i + 1;
                if (slotIndex >= _neededHeroesAmount) break;
                
                var playerData = _playerStoreManager.GetPlayerDataFromId(heroIds[i]);
                if (playerData == null) continue;

                _selectedHeroes[slotIndex] = playerData;
                fusionAvatars[slotIndex].SetData(playerData);
                fusionItemDisplays[slotIndex].SetData(playerData);
            }
            
            SelectItem(0); // select base hero by default
            fusionBtn.interactable = CanFusion();
            RenderItemsPercents();
        }

        private void SelectItem(int index) {
            if (index < 0 || index >= _neededHeroesAmount) {
                return;
            }
            fusionItemDisplays.ForEach(e=>e.SetChoose(false));
            fusionItemDisplays[index].SetChoose(true);
        }

        private void RenderItemsPercents() {
            fusionPercentDisplays.ForEach(e => e.gameObject.SetActive(false));
            heroSObjects.ForEach(e => e.SetActive(false));
            fusionStars.ForEach(e=>e.color = defaultStarColor);

            if (_mode == FusionMode.Level) {
                // In evolution mode, we don't use standard percent displays
                return;
            }

            if (!CanFusion()) {
                return;
            }

            heroSObjects.ForEach(e => e.SetActive(true));
            for (var i = 0; i < fusionStars.Count; i++) {
                fusionStars[i].color = rarityColor[_selectedHeroes[i].rare];
            }
            var data = _selectedHeroes.GroupBy(e => e.rare);
            var index = 0;
            foreach (var d in data) {
                var percent = d.Count() * PERCENT;
                var f =fusionPercentDisplays[d.Key]; 
                f.gameObject.SetActive(true);
                f.Init(percent);
                index++;
            }
        }

        private bool CanFusion() {
            return _selectedHeroes.Count(e => e != null) == _neededHeroesAmount;
        }

        public void Show(Canvas canvas) {
            base.Show(canvas);
        }
    }
}