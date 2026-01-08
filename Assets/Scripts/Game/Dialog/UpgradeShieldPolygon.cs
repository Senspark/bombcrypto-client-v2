using System;
using System.Linq;
using Animation;
using App;
using Cysharp.Threading.Tasks;
using Engine.Manager;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class UpgradeShieldPolygon : MonoBehaviour {
        private static int MaxShieldLevel = 4;

        [SerializeField]
        private Avatar resetThisHeroAvatar;

        [SerializeField]
        private Image backlight;

        [SerializeField]
        private Text heroIdLbl;

        [SerializeField]
        private Text heroCurrentShieldAmountLbl;

        [SerializeField]
        private Text heroNextShieldAmountLbl;

        [SerializeField]
        private Button resetBtn;

        [SerializeField]
        private Text amountMaterialRepair;

        [SerializeField]
        private GameObject groupPlus;

        [SerializeField]
        private GameObject avatar;

        [SerializeField]
        private Image[] stars;

        [SerializeField]
        private Sprite[] starSprites;

        [SerializeField]
        private GameObject index;

        private PlayerData _resetThisHero;
        private Action<PlayerData> _chooseHeroCallBack;

        private ISoundManager _soundManager;
        private IPlayerStorageManager _playerStoreManager;
        private IStorageManager _storeManager;
        private IBlockchainManager _blockchainManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IServerManager _serverManager;
        private IChestRewardManager _chestRewardManager;

        private Canvas _canvas;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            resetBtn.interactable = false;
            AddEvent();
        }

        private void OnDestroy() {
            RemoveEvent();
        }

        private void AddEvent() {
            EventManager<PlayerData>.Add(StakeEvent.AfterStake, ResetUiAfterStake);
        }
        private void RemoveEvent() {
            EventManager<PlayerData>.Remove(StakeEvent.AfterStake, ResetUiAfterStake);
        }

        public void SetInfo(Canvas canvas, Action<PlayerData> chooseHeroCallBack) {
            _canvas = canvas;
            _chooseHeroCallBack = chooseHeroCallBack;
        }

        public void OnUpgradeShieldBtnClicked() {
            if (!CanUpgradeShield()) {
                return;
            }
            resetBtn.interactable = false;
            _soundManager.PlaySound(Audio.Tap);
            var idHeroS = _resetThisHero.heroId;

            UniTask.Void(async () => {
                var waiting = await DialogWaiting.Create();
                waiting.Show(_canvas);
                waiting.ShowLoadingAnim();
                
                try {
                    var nextLevelShield = _resetThisHero.levelShield + 1;
                    //Bước 1: Gửi yêu cầu upgrade level shield cho server để trừ đá
                    var response = await _serverManager.General.UpgradeLevelShield(idHeroS);
                    //Bước 2: Gửi yêu cầu upgrade level shield cho blockchain sau khi đã có lệnh từ server
                    var result =
                        await _blockchainManager.UpgradeShieldLevelV2(idHeroS.Id, response.Nonce, response.Signature);
                    if (!result) {
                        throw new Exception("Upgrade Failed");
                    }
                    //Bước 3: Đồng bộ với server
                    await _serverManager.General.SyncHero(false);
                    var newData = _playerStoreManager.GetPlayerDataFromId(idHeroS);
                    while (newData.levelShield != nextLevelShield) {
                        await WebGLTaskDelay.Instance.Delay(10000);
                        await _serverManager.General.SyncHero(false);
                        newData = _playerStoreManager.GetPlayerDataFromId(idHeroS);
                    }
                    Init(newData);
                    DialogForge.ShowInfo(_canvas, "Successfully");
                    UpdateUI();
                    _chooseHeroCallBack?.Invoke(newData);
                    _resetThisHero = newData;
                } catch (Exception e) {
                    resetBtn.interactable = CanUpgradeShield();
                    DialogForge.ShowError(_canvas, e.Message);
                } finally {
                    waiting.Hide();
                }
            });
        }

        public async void ChooseResetHero() {
            var inventory = await DialogInventoryCreator.Create();
            var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !CanProcessThisHero(e))
                .Select(e => e.heroId).ToArray();
            inventory.SetChooseHeroForResetRoi(exclude, DisplayResetHeroWithId);
            inventory.Show(_canvas);
        }

        public async void Init(PlayerData resetThisHero) {
            _resetThisHero = !CanProcessThisHero(resetThisHero) ? null : resetThisHero;

            if (_resetThisHero != null) {
                heroIdLbl.text = _resetThisHero.heroId.Id.ToString();
                backlight.sprite = await AnimationResource.GetBacklightImageByRarity(_resetThisHero.rare, true);
                backlight.enabled = true;
                resetThisHeroAvatar.ChangeImage(_resetThisHero);
                groupPlus.gameObject.SetActive(false);
                avatar.gameObject.SetActive(true);
                //amountMaterialRepair.gameObject.SetActive(true);
                UpdateUI();
            } else {
                heroIdLbl.text = string.Empty;
                heroCurrentShieldAmountLbl.text = string.Empty;
                heroNextShieldAmountLbl.text = string.Empty;
                backlight.sprite = null;
                backlight.enabled = false;
                resetThisHeroAvatar.HideImage();
                groupPlus.gameObject.SetActive(true);
                avatar.gameObject.SetActive(false);
                //amountMaterialRepair.gameObject.SetActive(false);
                amountMaterialRepair.text = "--";
            }
        }

        private void DisplayResetHeroWithId(HeroId heroId) {
            var playerData = _playerStoreManager.GetPlayerDataFromId(heroId);
            Init(playerData);
            _chooseHeroCallBack?.Invoke(playerData);
        }
        
        //Sau khi unstake ko còn là hero S nữa, remove và xoá khỏi ui
        private void ResetUiAfterStake(PlayerData player) {
            if (player.Shield == null) {
                DisplayResetHeroWithId(player.heroId);
                amountMaterialRepair.text = "--";
                resetBtn.interactable = false;
            }

        }

        private void UpdateUI() {
            var heroType = _playerStoreManager.GetHeroRarity(_resetThisHero);
            var shieldLevel = _resetThisHero.levelShield +1 >= MaxShieldLevel ? MaxShieldLevel - 1: _resetThisHero.levelShield + 1;
            var amountRockNeedExchange =
                _storeManager.UpgradeShieldConfig.PriceRock[heroType][shieldLevel];
            var currentShieldAmount = _resetThisHero.Shield.TotalAmount;
            var nextShieldAmount = ShieldAmountNextLevel(heroType, _resetThisHero.levelShield);

            heroCurrentShieldAmountLbl.text = $"{currentShieldAmount}";
            heroNextShieldAmountLbl.text = $"{nextShieldAmount}";
            amountMaterialRepair.text = $"{amountRockNeedExchange}";
            resetBtn.interactable = CanUpgradeShield();
            index.gameObject.SetActive(_resetThisHero.levelShield < MaxShieldLevel - 1);
            //amountMaterialRepair.gameObject.SetActive(_resetThisHero.levelShield < MaxShieldLevel - 1);
            if (_resetThisHero.levelShield > MaxShieldLevel - 1) {
                amountMaterialRepair.text = "--";
            }

            ShowStars();
        }

        private void ShowStars() {
            var levelShield = _resetThisHero.levelShield + 1;
            for (var i = 0; i < stars.Length; i++) {
                stars[i].sprite = starSprites[0];
                if (i < levelShield) {
                    stars[i].sprite = starSprites[1];
                }
                if (i == levelShield) {
                    stars[i].sprite = starSprites[2];
                }
            }
        }

        private static bool CanProcessThisHero(PlayerData hero) {
            if (hero == null || (!hero.IsHeroS && hero.Shield == null) || hero.AccountType == HeroAccountType.Trial) {
                return false;
            }
            return true;
        }

        private bool CanUpgradeShield() {
            if (!CanProcessThisHero(_resetThisHero)) {
                return false;
            }

            var validLevelShield = _resetThisHero.levelShield < MaxShieldLevel - 1;
            if (!validLevelShield) {
                return false;
            }
            var heroRarity = _playerStoreManager.GetHeroRarity(_resetThisHero);
            var amountExchange =
                _storeManager.UpgradeShieldConfig.PriceRock[heroRarity][_resetThisHero.levelShield + 1];
            return amountExchange <= _chestRewardManager.GetRock();
        }

        private int ShieldAmountNextLevel(HeroRarity heroType, int level) {
            var nextLevel = level + 1;
            var validLevelShield = nextLevel < MaxShieldLevel;
            if (!validLevelShield) {
                return 0;
            }
            return _storeManager.UpgradeShieldConfig.DurabilityPoint[heroType][nextLevel];
        }
    }
}