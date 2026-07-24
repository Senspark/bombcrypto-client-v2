using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Animation;
using App;
using Cysharp.Threading.Tasks;
using Engine.Manager;
using Game.Manager;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class RepairShieldPolygon : MonoBehaviour {
        [SerializeField]
        private Avatar resetThisHeroAvatar;

        [SerializeField]
        private Image backlight;

        [SerializeField]
        private Text heroIdLbl;

        [SerializeField]
        private Text heroShieldAmountLbl;

        [SerializeField]
        private Button resetBtn;

        [SerializeField]
        private Text amountMaterialRepair;

        [SerializeField]
        private GameObject groupPlus;

        [SerializeField]
        private GameObject avatar;
        
        [SerializeField]
        protected Button senBtn;

        protected PlayerData ResetThisHero;

        // Heroes selected in multi-select mode (batch reset). Null/<=1 => single-hero flow.
        private List<PlayerData> _selectedHeroes;

        protected Canvas Canvas;

        protected ISoundManager SoundManager;
        private IBHeroManager _playerStoreManager;
        private IStorageManager _storeManager;
        protected ILanguageManager _languageManager;
        private IOnBoardingManager _onBoardingManager;
        protected NewRepairShieldController Controller;
        private Action<PlayerData> _chooseHeroCallBack;

        private void Awake() {
            SoundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();

            var blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            var blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            var chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            Controller = new NewRepairShieldController(_playerStoreManager, _storeManager, blockchainManager,
                serverManager, chestRewardManager, blockchainStorageManager);
            OnAwake();
            UpdateUI();
            AddEvent();
        }

        private void AddEvent() {
            EventManager<PlayerData>.Add(StakeEvent.AfterStake, ResetUiAfterStake);
        }
        private void RemoveEvent() {
            EventManager<PlayerData>.Remove(StakeEvent.AfterStake, ResetUiAfterStake);
        }
        

        #region PUBLIC

        public async void OnResetShieldBtnClicked() {
            // Batch reset when more than one hero is selected (outside onboarding)
            if (!CheckOnBoarding() && _selectedHeroes != null && _selectedHeroes.Count > 1) {
                OnResetMultipleClicked();
                return;
            }

            resetBtn.interactable = false;
            SoundManager.PlaySound(Audio.Tap);
            var hero = ResetThisHero;
            
            
            void OnYes() {
                senBtn.interactable = false;
                if (CheckOnBoarding()) {
                    _onBoardingManager.DispatchEvent(e => e.updateOnBoarding?.Invoke(TutorialStep.DoneRepairShield));
                    OnResetCompleted(hero);
                    return;
                }
                
                var waiting = new WaitingUiManager(Canvas);
                waiting.Begin();
                UniTask.Void(async () => {
                    try {
                        var newData = await Controller.ProcessUsingMaterial(ResetThisHero);
                        OnResetCompleted(newData);
                    } catch (Exception e) {
                        resetBtn.interactable = Controller.CanProcessUsingMaterial(ResetThisHero) || CheckOnBoarding();
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(Canvas, e);
                        } else {
                            DialogOK.ShowError(Canvas, e);
                        }
                    }
                    waiting.End();
                });
            }
            
            var fee = Controller.RateExchangeMaterialsToHero(hero);
            if (CheckOnBoarding()) {
                fee = 0;
            }
            var info = _languageManager.GetValue(LocalizeKey.ui_info_buy_repair_shield);
            var str = string.Format(info, fee, "Quartz");
            var dialog = await DialogConfirm.Create();
            dialog.SetInfo(str, "Yes", "No", OnYes, ()=> {
                resetBtn.interactable = Controller.CanProcessUsingMaterial(ResetThisHero) || CheckOnBoarding();
            }).Show(Canvas);
        }

        public async void ChooseResetHero() {
            var inventory = await DialogInventoryCreator.Create();
            var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !Controller.CanHeroRepairable(e) || !Controller.HasRepairConfig(e))
                .Select(e => e.heroId).ToArray();

            // During onboarding keep the single-hero flow (the tutorial expects one hero at a time)
            if (CheckOnBoarding()) {
                Array.Clear(exclude, 0, exclude.Length);
                inventory.SetChooseHeroForResetRoi(exclude, DisplayResetHeroWithId);
                inventory.Show(Canvas);
                return;
            }

            // Outside onboarding: dedicated multi-select (checkboxes, without burn/stake warnings)
            inventory.SetChooseHeroForRepairShield(exclude, OnHeroesSelected);
            inventory.Show(Canvas);
        }

        private async void OnHeroesSelected(PlayerData[] heroes) {
            _selectedHeroes = heroes
                .Where(e => e != null && Controller.CanHeroRepairable(e) && Controller.HasRepairConfig(e))
                .ToList();

            if (_selectedHeroes.Count == 0) {
                Init(null);
                _chooseHeroCallBack?.Invoke(null);
                return;
            }

            // A single hero => behaves like the single-hero flow
            if (_selectedHeroes.Count == 1) {
                DisplayResetHeroWithId(_selectedHeroes[0].heroId);
                return;
            }

            await ShowMultiSelectionSummary();
        }

        private async Task ShowMultiSelectionSummary() {
            var first = _selectedHeroes[0];
            ResetThisHero = first;

            // Count first: doesn't depend on config, so it never breaks the screen
            heroIdLbl.text = $"x{_selectedHeroes.Count}";
            heroShieldAmountLbl.text = $"{_selectedHeroes.Count} heroes";

            backlight.sprite = await AnimationResource.GetBacklightImageByRarity(first.rare, true);
            backlight.enabled = true;
            resetThisHeroAvatar.ChangeImage(first);
            groupPlus.gameObject.SetActive(false);
            avatar.gameObject.SetActive(true);

            var totalCost = Controller.TotalRepairCostUsingMaterial(_selectedHeroes);
            amountMaterialRepair.text = $"{totalCost}";
            resetBtn.interactable = Controller.CanProcessBatchUsingMaterial(_selectedHeroes);
        }

        private async void OnResetMultipleClicked() {
            resetBtn.interactable = false;
            SoundManager.PlaySound(Audio.Tap);
            var heroes = _selectedHeroes.ToList();
            var totalCost = Controller.TotalRepairCostUsingMaterial(heroes);

            void OnYes() {
                var waiting = new WaitingUiManager(Canvas);
                waiting.Begin();
                UniTask.Void(async () => {
                    try {
                        var result = await Controller.ProcessBatchUsingMaterial(heroes);
                        OnBatchResetCompleted(result);
                    } catch (Exception e) {
                        resetBtn.interactable = Controller.CanProcessBatchUsingMaterial(_selectedHeroes);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(Canvas, e);
                        } else {
                            DialogOK.ShowError(Canvas, e);
                        }
                    } finally {
                        waiting.End();
                    }
                });
            }

            var info = _languageManager.GetValue(LocalizeKey.ui_info_buy_repair_shield);
            var str = $"{heroes.Count} heroes\n" + string.Format(info, totalCost, "Quartz");
            var dialog = await DialogConfirm.Create();
            dialog.SetInfo(str, "Yes", "No", OnYes, () => {
                resetBtn.interactable = Controller.CanProcessBatchUsingMaterial(_selectedHeroes);
            }).Show(Canvas);
        }

        private void OnBatchResetCompleted(IRepairShieldBatchResponse result) {
            _selectedHeroes = null;
            Init(null);
            var message = result.FailedCount > 0
                ? $"Repaired {result.SuccessCount}, failed {result.FailedCount}"
                : $"Successfully repaired {result.SuccessCount} heroes";
            DialogOK.ShowInfo(Canvas, message);
            _chooseHeroCallBack?.Invoke(null);
        }

        public async void Init(PlayerData resetThisHero) {
            ResetThisHero = Controller.IsValidHero(resetThisHero) ? resetThisHero : null;
            if (ResetThisHero != null) {
                heroIdLbl.text = ResetThisHero.heroId.Id.ToString();
                backlight.sprite = await AnimationResource.GetBacklightImageByRarity(ResetThisHero.rare, true);
                backlight.enabled = true;
                resetThisHeroAvatar.ChangeImage(ResetThisHero);
                groupPlus.gameObject.SetActive(false);
                avatar.gameObject.SetActive(true);
                UpdateUI();
            } else {
                heroIdLbl.text = string.Empty;
                heroShieldAmountLbl.text = string.Empty;
                backlight.sprite = null;
                backlight.enabled = false;
                resetThisHeroAvatar.HideImage();
                groupPlus.gameObject.SetActive(true);
                avatar.gameObject.SetActive(false);
            }
        }

        public void SetInfo(Canvas canvas, Action<PlayerData> chooseHeroCallBack) {
            Canvas = canvas;
            _chooseHeroCallBack = chooseHeroCallBack;
        }

        #endregion

        #region PROTECTED

        protected virtual void OnAwake() {
        }

        protected void OnResetCompleted(PlayerData newData) {
            Init(newData);
            DialogOK.ShowInfo(Canvas, "Successfully");
            UpdateUI();
            _chooseHeroCallBack?.Invoke(newData);
            ResetThisHero = newData;
        }

        protected virtual void UpdateUI() {
            var hero = ResetThisHero;
            if (hero != null) {
                var currentAmount = hero.Shield.CurrentAmount;
                var totalAmount = hero.Shield.TotalAmount;
                var amountRockNeedExchange = Controller.RateExchangeMaterialsToHero(hero);
                if (CheckOnBoarding()) {
                    amountRockNeedExchange = 0;
                    var fakeShieldAmount = UnityEngine.Random.Range(0.6f, 0.7f) * totalAmount;
                    currentAmount = Mathf.RoundToInt(fakeShieldAmount);
                }
                heroShieldAmountLbl.text = $"{currentAmount}/{totalAmount}";
                amountMaterialRepair.text = $"{amountRockNeedExchange}";
                resetBtn.interactable = Controller.CanProcessUsingMaterial(hero) || CheckOnBoarding();
            } else {
                amountMaterialRepair.text = "--";
                resetBtn.interactable = false;
            }
        }

        #endregion

        //Sau khi unstake ko còn là hero S nữa, remove và xoá khỏi ui
        private void ResetUiAfterStake(PlayerData player) {
            if (player.Shield == null) {
                DisplayResetHeroWithId(player.heroId);
                amountMaterialRepair.text = "--";
                resetBtn.interactable = false;
            }
        }

        private void DisplayResetHeroWithId(HeroId heroId) {
            var playerData = _playerStoreManager.GetPlayerDataFromId(heroId);
            Init(playerData);
            _chooseHeroCallBack.Invoke(playerData);
        }
        
        private bool CheckOnBoarding() {
            return _onBoardingManager.CurrentStep == TutorialStep.RepairShield;
        }

        private void OnDestroy() {
            RemoveEvent();
        }
    }

    public class NewRepairShieldController {
        private readonly IBHeroManager _playerStoreManager;
        private readonly IStorageManager _storeManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IServerManager _serverManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private readonly IOnBoardingManager _onBoardingManager;

        public NewRepairShieldController(
            IBHeroManager bHeroManager,
            IStorageManager storageManager,
            IBlockchainManager blockchainManager,
            IServerManager serverManager,
            IChestRewardManager chestRewardManager,
            IBlockchainStorageManager blockchainStorageManager
        ) {
            _playerStoreManager = bHeroManager;
            _storeManager = storageManager;
            _blockchainManager = blockchainManager;
            _serverManager = serverManager;
            _chestRewardManager = chestRewardManager;
            _blockchainStorageManager = blockchainStorageManager;
        }
        
        public int RateExchangeMaterialsToHero(PlayerData hero) {
            var rarity = hero.rare;
            var level = hero.levelShield;
            var shieldConfig = _storeManager.RepairShieldConfig.Data[rarity][level];
            return (int)shieldConfig;
        }

        // Is there a repair-cost config for this hero's rarity/level?
        // (heroes with a new rarity/level without config can't be repaired)
        public bool HasRepairConfig(PlayerData hero) {
            if (hero == null) {
                return false;
            }
            try {
                var config = _storeManager.RepairShieldConfig?.Data;
                return config != null
                       && config.ContainsKey(hero.rare)
                       && config[hero.rare].ContainsKey(hero.levelShield);
            } catch {
                return false;
            }
        }
        
        public int SenFeeRepairShield(PlayerData hero) {
            var heroType = _playerStoreManager.GetHeroRarity(hero);
            return heroType switch {
                HeroRarity.Common => 10,
                HeroRarity.Rare => 10,
                HeroRarity.SuperRare => 20,
                HeroRarity.Epic => 30,
                HeroRarity.Legend => 40,
                HeroRarity.SuperLegend => 50,
                _ => throw new ArgumentOutOfRangeException(nameof(heroType), heroType, null)
            };
        }

        public bool IsValidHero(PlayerData hero) {
            return hero != null && (hero.IsHeroS || hero.Shield != null)  && hero.AccountType == HeroAccountType.Nft;
        }

        public bool CanHeroRepairable(PlayerData hero) {
            if (!IsValidHero(hero)) {
                return false;
            }

            var validShieldAmount = hero.Shield.CurrentAmount < hero.Shield.TotalAmount;
            return validShieldAmount;
        }

        public bool CanProcessUsingMaterial(PlayerData hero) {
            if (!CanHeroRepairable(hero)) {
                return false;
            }
            
            var fee = RateExchangeMaterialsToHero(hero);
            var current = _chestRewardManager.GetRock();
            return fee <= current;
        }
        
        public bool CanProcessUsingSen(PlayerData hero) {
            if (!CanHeroRepairable(hero)) {
                return false;
            }

            var fee = SenFeeRepairShield(hero);
            var depositedSens = _chestRewardManager.GetSenRewardAndDeposit();
            return depositedSens >= fee;
        }

        public async Task<PlayerData> ProcessUsingMaterial(PlayerData hero) {
            if (!CanProcessUsingMaterial(hero)) {
                throw new Exception("Cannot repair");
            }
            
            var heroId = hero.heroId;
            await _serverManager.General.RepairShield(heroId, BlockRewardType.Rock);
            var newData = _playerStoreManager.GetPlayerDataFromId(heroId);
            return newData;
        }

        // Total cost in Quartz, ignoring heroes without a repair config (avoids KeyNotFound)
        public int TotalRepairCostUsingMaterial(List<PlayerData> heroes) {
            if (heroes == null) {
                return 0;
            }
            var total = 0;
            foreach (var hero in heroes) {
                if (hero == null || !CanHeroRepairable(hero)) {
                    continue;
                }
                try {
                    total += RateExchangeMaterialsToHero(hero);
                } catch (Exception) {
                    // hero without a repair config (e.g. new rarity/level) — skip it in the cost
                }
            }
            return total;
        }

        public bool CanProcessBatchUsingMaterial(List<PlayerData> heroes) {
            if (heroes == null || heroes.Count == 0) {
                return false;
            }
            var totalFee = TotalRepairCostUsingMaterial(heroes);
            if (totalFee <= 0) {
                return false;
            }
            var current = _chestRewardManager.GetRock();
            return totalFee <= current;
        }

        public async Task<IRepairShieldBatchResponse> ProcessBatchUsingMaterial(List<PlayerData> heroes) {
            var ids = heroes
                .Where(h => h != null && CanHeroRepairable(h))
                .Select(h => h.heroId.Id)
                .ToArray();
            if (ids.Length == 0) {
                throw new Exception("No heroes to repair");
            }
            return await _serverManager.General.RepairShieldBatch(ids, BlockRewardType.Rock);
        }

        public async Task<PlayerData> ProcessUsingSen(PlayerData hero) {
            if (!CanProcessUsingSen(hero)) {
                throw new Exception("Cannot repair");
            }
            
            var heroId = hero.heroId;
            await _serverManager.General.RepairShield(heroId, BlockRewardType.Senspark);
            var newData = _playerStoreManager.GetPlayerDataFromId(heroId);
            return newData;
        }
    }
}