using System;
using System.Linq;

using Analytics;

using App;

using BLPvpMode.Engine.Info;
using BLPvpMode.UI;

using BomberLand.Button;
using BomberLand.Component;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
using Game.UI.Animation;

using PvpMode.Manager;

using Scenes.MainMenuScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Senspark;

using Services;
using Services.IapAds;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using AdResult = Services.IapAds.AdResult;

namespace Scenes.PvpModeScene.Scripts {
    public class BLDialogPvpWin : Dialog {
        [SerializeField]
        private Transform rewardContainer;

        [SerializeField]
        private WinReward rewardPrefab;

        [SerializeField]
        private ButtonZoomAndFlash buttonAds;

        [SerializeField]
        private GameObject adsButtons;

        [SerializeField]
        private ButtonZoomAndFlash nextButton;

        [SerializeField]
        private Button smallNextButton;

        [SerializeField]
        private Text nextText;

        [SerializeField]
        private Text openChestText;

        [SerializeField]
        private Button buttonLuckyWheelAds;

        [SerializeField]
        private ButtonWithAlarm buttonLuckyWheelGold;

        [SerializeField]
        private GameObject winInfo;

        [SerializeField]
        private GameObject score;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private GameObject boosterContent;

        [SerializeField]
        private Image boosterIcon;

        [SerializeField]
        private TextMeshProUGUI boosterText;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private GameObject body;

        private IServerManager _serverManager;
        private IUnityAdsManager _unityAdsManager;
        private IAnalytics _analytics;
        private IStorageManager _storageManager;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private IServerRequester _serverRequester;
        private IProductItemManager _productItemManager;
        private IInventoryManager _inventoryManager;
        private IInputManager _inputManager;

        private Action _onNextCallback;
        private WinReward _goldReward;
        private string _pvpRewardId;
        public bool _isShowDone = false;
        private bool _isSpinLuckyWheel = false;

        private InventoryChestData _chestData;
        private GachaChestItemData[] _itemsReward;

        public static UniTask<BLDialogPvpWin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvpWin>();
        }

        protected override void Awake() {
            base.Awake();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();

            adsButtons.SetActive(GameConstant.EnableAdsPvp);
            var blGold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            buttonLuckyWheelGold.SetGrey(blGold < 100);
        }

        protected override void ExtraCheck() {
            if (Input.GetKeyDown(KeyCode.S) || _inputManager.ReadJoystick(_inputManager.InputConfig.Back)) {
                if (buttonLuckyWheelGold.Interactable && buttonLuckyWheelGold.gameObject.activeInHierarchy && !_isSpinLuckyWheel) {
                    OnBtLuckyWheel();
                }
            }
        }

        protected override void OnYesClick() {
            if (!nextButton.gameObject.activeInHierarchy || !nextButton.Interactable || !_isShowDone) {
                return;
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter) && !_isSpinLuckyWheel) {
                OnNextClicked();
            }
        }

        protected override void OnNoClick() {
            //do nothing
        }

        public void SetRewards(
            IPvpResultUserInfo userInfo,
            string rewardId,
            bool isOutOfChest,
            System.Action callback
        ) {
            winInfo.SetActive(true);
            score.SetActive(false);
            _onNextCallback = callback;

            _pvpRewardId = rewardId;
            CreateRewardItem(RewardSourceType.Rank, userInfo.DeltaPoint, false);
            var hadChest = false;
            foreach (var iter in userInfo.Rewards.Keys) {
                var type = RewardResource.ConvertIdToEnum(iter);
                var reward = CreateRewardItem(type,
                    (int)userInfo.Rewards[iter],
                    isOutOfChest);
                if (type == RewardSourceType.Gold) {
                    _goldReward = reward;
                }
                if (type is RewardSourceType.BronzeChest or
                    RewardSourceType.SilverChest or
                    RewardSourceType.GoldChest or
                    RewardSourceType.PlatinumChest) {
                    hadChest = true;
                }
            }
            if (hadChest) {
                SetOpenChest();
            } else {
                nextButton.Interactable = true;
            }
            if (_goldReward != null && _goldReward.Value > 0) {
                // Have Gold
                _analytics.Pvp_TrackSoftCurrencyByWin(_goldReward.Value);
            }
            if (Application.isMobilePlatform) {
                if (GameConstant.EnableLuckyWheelPvp) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(true);
                    buttonLuckyWheelGold.gameObject.SetActive(false);
                    buttonAds.gameObject.SetActive(false);
                } else if (_goldReward != null && _goldReward.Value > 0) {
                    // Show Bt Ads
                    if (GameConstant.EnableAdsPvp) {
                        adsButtons.SetActive(true);
                        buttonAds.gameObject.SetActive(false);
                        buttonLuckyWheelAds.gameObject.SetActive(false);
                        buttonLuckyWheelGold.gameObject.SetActive(false);
                    } else {
                        adsButtons.SetActive(false);
                        nextButton.gameObject.SetActive(true);
                    }
                } else {
                    adsButtons.SetActive(false);
                    nextButton.gameObject.SetActive(true);
                }
            } else {
                if (GameConstant.EnableLuckyWheelPvp) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(false);
                    buttonLuckyWheelGold.gameObject.SetActive(true);
                    buttonAds.gameObject.SetActive(false);
                } else {
                    adsButtons.SetActive(false);
                    nextButton.gameObject.SetActive(true);
                }
            }
        }

        private void SetOpenChest() {
            nextButton.Interactable = false;
            nextText.gameObject.SetActive(false);
            openChestText.gameObject.SetActive(true);
            _chestData = null;
            UniTask.Void(async () => {
                var result = await _inventoryManager.GetChestAsync();
                var chestList = result.ToArray();
                if (chestList.Length == 0) {
                    nextButton.Interactable = true;
                    return;
                }
                _chestData = chestList[0];
                nextButton.Interactable = true;
                openChestText.text = $"OPEN {_chestData.ChestName.ToUpper()}";
                // Nhằm tránh mất rương vì lý do nào đó user không nhấn nút open chest mà thoát app
                // Tự động mở rương trước sau đó nút open chest chỉ trình diễn animation.
                _itemsReward = await _serverRequester.OpenGachaChest(_productItemManager, _chestData.ChestId);
                TrackOpenChest(_chestData, _itemsReward);
                await _serverManager.General.GetChestReward();
            });
        }

        private void TrackOpenChest(InventoryChestData chestData, GachaChestItemData[] rewards) {
            var productIds = rewards.Select(e => e.ProductId.ToString()).ToArray();
            _analytics.Inventory_TrackOpenChestByGem(
                chestData.ChestName,
                chestData.ChestId,
                productIds,
                rewards.Select(e => e.Value).ToArray()
            );
            _analytics.TrackConversion(ConversionType.OpenChest);
        }

        public async void UpdateBooster(int[] boosterIds) {
            boosterContent.SetActive(false);
            foreach (var boosterId in boosterIds) {
                var type = DefaultBoosterManager.ConvertFromId(boosterId);
                switch (type) {
                    case BoosterType.CupBonus:
                        boosterIcon.sprite = await resource.GetSpriteByItemId(boosterId);
                        boosterText.text = "+50%";
                        boosterContent.SetActive(true);
                        return;
                    case BoosterType.FullCupBonus:
                        boosterIcon.sprite = await resource.GetSpriteByItemId(boosterId);
                        boosterText.text = "+100%";
                        boosterContent.SetActive(true);
                        return;
                }
            }
        }

        public void SetTournamentResult(int slot, IPvpResultInfo info, System.Action callback) {
            Assert.IsTrue(info.Scores.Length == 2);
            winInfo.SetActive(false);
            score.SetActive(true);
            _onNextCallback = callback;
            scoreText.text = $"{info.Scores[slot]}-{info.Scores[1 - slot]}";
            nextButton.gameObject.SetActive(true);
            adsButtons.SetActive(false);
        }

        private WinReward CreateRewardItem(RewardSourceType type, int value, bool outOfSlot) {
            var item = Instantiate(rewardPrefab, rewardContainer, false);
            var fullSlot = RewardIsChest(type) && outOfSlot;
            item.SetInfo(type, value, fullSlot);
            return item;
        }

        private bool RewardIsChest(RewardSourceType type) {
            return type switch {
                RewardSourceType.Rank => false,
                RewardSourceType.Gold => false,
                _ => true
            };
        }

        public void OnNextClicked() {
            nextButton.Interactable = false;
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);

            if (_chestData != null) {
                body.SetActive(false);
                OpenChest();
                return;
            }
            _onNextCallback?.Invoke();
            Hide();
        }

        private async void OpenChest() {
            var dialog = await BLDialogGachaChest.CreateFromChestInventory(_itemsReward, _chestData);
            dialog.OnDidHide(() => {
                _onNextCallback?.Invoke();
                Hide();
            });
            dialog.Show(DialogCanvas);
        }

        public void OnAdsClicked() {
            buttonAds.Interactable = false;
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    _analytics.TrackAds(AdsCategory.X2GoldPvp, TrackAdsResult.Start);

                    var adsId = await _unityAdsManager.ShowRewarded();
                    var result = await _serverManager.Pvp.GetBonusRewardPvp(_storageManager.PvpMatchId, adsId);

                    _analytics.Pvp_TrackSoftCurrencyByX2Gold(result.Value);
                    _analytics.TrackAds(AdsCategory.X2GoldPvp, TrackAdsResult.Complete);
                    _analytics.TrackConversion(ConversionType.X2GoldPvpWatchAds);

                    ShowDialogBonus(_goldReward.Value + result.Value);
                    _goldReward.AddValue(result.Value);
                } catch (Exception e) {
                    if (e is AdException adException) {
                        var adsResult = adException.Result switch {
                            AdResult.Cancel => TrackAdsResult.Cancel,
                            _ => TrackAdsResult.Error
                        };
                        _analytics.TrackAds(AdsCategory.X2GoldPve, adsResult);
                    }
                    buttonAds.Interactable = true;
                    DialogOK.ShowError(DialogCanvas, e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        private void ShowDialogBonus(int value) {
            buttonAds.gameObject.SetActive(false);
            DialogBonusReward.Create().ContinueWith(dialog => {
                dialog.SetInfo(value);
                dialog.Show(DialogCanvas);
            });
        }
        

        public void OnBtLuckyWheel() {
            
            buttonLuckyWheelAds.interactable = false;
            nextButton.Interactable = false;
            if (!Application.isMobilePlatform) {
                var blGold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
                if (blGold < 100) {
                    buttonLuckyWheelGold.ShowAlarm(DialogCanvas);
                    nextButton.Interactable = true;
                    return;
                }
            }
            _isSpinLuckyWheel = true;
            BLDialogLuckyWheel.Create().ContinueWith(dialog => {
                dialog.SetData(DialogCanvas, adsButtons, _pvpRewardId, true, "win",
                    () => {
                        adsButtons.SetActive(false);
                        nextButton.Interactable = true;
                        _isSpinLuckyWheel = false;
                    }
                );
                dialog.Show(DialogCanvas);
            });
        }
    }
}