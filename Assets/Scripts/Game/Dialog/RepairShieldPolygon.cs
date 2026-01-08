using System;
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
        protected Canvas Canvas;

        protected ISoundManager SoundManager;
        private IPlayerStorageManager _playerStoreManager;
        private IStorageManager _storeManager;
        protected ILanguageManager _languageManager;
        private IOnBoardingManager _onBoardingManager;
        protected NewRepairShieldController Controller;
        private Action<PlayerData> _chooseHeroCallBack;

        private void Awake() {
            SoundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
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
                            DialogError.ShowError(Canvas, e.Message);
                        } else {
                            DialogOK.ShowError(Canvas, e.Message);
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
                .Where(e => !Controller.CanHeroRepairable(e))
                .Select(e => e.heroId).ToArray();
            if (CheckOnBoarding()) {
                Array.Clear(exclude, 0, exclude.Length);
            }
            inventory.SetChooseHeroForResetRoi(exclude, DisplayResetHeroWithId);
            inventory.Show(Canvas);
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
        private readonly IPlayerStorageManager _playerStoreManager;
        private readonly IStorageManager _storeManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IServerManager _serverManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private readonly IOnBoardingManager _onBoardingManager;

        public NewRepairShieldController(
            IPlayerStorageManager playerStorageManager,
            IStorageManager storageManager,
            IBlockchainManager blockchainManager,
            IServerManager serverManager,
            IChestRewardManager chestRewardManager,
            IBlockchainStorageManager blockchainStorageManager
        ) {
            _playerStoreManager = playerStorageManager;
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