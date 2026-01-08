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

        private const int NEED_HEROES_AMOUNT = 5;
        private const int PERCENT = 20;
        private PlayerData[] _selectedHeroes;
        private int _waitingItemIndex = -1;
        
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

            Init();
            RenderItemsPercents();
        }

        public void OnFusionBtnClicked() {
            if (!CanFusion()) {
                return;
            }
            fusionBtn.interactable = false;
            _soundManager.PlaySound(Audio.Tap);
            var heroIds = _selectedHeroes.Select(e => e.heroId.Id).ToArray();
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            waiting.ChangeText("Processing...");

            UniTask.Void(async () => {
                try {
                    var result = await _blockchainManager.FusionHero(heroIds);
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
            
            if (_waitingItemIndex < 0 || _waitingItemIndex >= NEED_HEROES_AMOUNT) {
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
        
        private void Init() {
            for (var i = 0; i < NEED_HEROES_AMOUNT; i++) {
                fusionItemDisplays[i].Init(i, ChooseHero);
                fusionItemDisplays[i].SetData(null);
                fusionAvatars[i].Init(i, ChooseHero, null);
                fusionAvatars[i].SetData(null);
            }
            _selectedHeroes = new PlayerData[NEED_HEROES_AMOUNT];

            heroDetailsDisplay.Hide();
            fusionBtn.interactable = false;
        }

        private void DisplayHeroWithId(HeroId heroId) {
            var playerData = _playerStoreManager.GetPlayerDataFromId(heroId);
            if (_waitingItemIndex < 0 || _waitingItemIndex >= NEED_HEROES_AMOUNT || playerData == null) {
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
            _waitingItemIndex = itemIndex;
            var selectedHero = _selectedHeroes[itemIndex]; 

            if (selectedHero == null) { 
                // Choose new hero
                var inventory = await DialogInventoryCreator.Create();
                var exclude = _selectedHeroes.Where(e => e != null).Select(e => e.heroId).ToArray();
                inventory.SetChooseHeroForResetRoi(exclude, DisplayHeroWithId);
                inventory.Show(DialogCanvas);
            } else {
                // display only
                SelectItem(itemIndex);
                heroDetailsDisplay.SetInfo(selectedHero, DialogCanvas);
                heroDetailsDisplay.Show();
            }
        }

        private void SelectItem(int index) {
            if (index < 0 || index >= NEED_HEROES_AMOUNT) {
                return;
            }
            fusionItemDisplays.ForEach(e=>e.SetChoose(false));
            fusionItemDisplays[index].SetChoose(true);
        }

        private void RenderItemsPercents() {
            fusionPercentDisplays.ForEach(e => e.gameObject.SetActive(false));
            heroSObjects.ForEach(e => e.SetActive(false));
            fusionStars.ForEach(e=>e.color = defaultStarColor);

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
            return _selectedHeroes.Count(e => e != null) == NEED_HEROES_AMOUNT;
        }

        public void Show(Canvas canvas) {
            base.Show(canvas);
        }
    }
}