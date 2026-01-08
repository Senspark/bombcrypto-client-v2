using System;
using System.Linq;

using Analytics;

using App;

using BomberLand.Button;
using BomberLand.Component;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using DG.Tweening;

using Game.Dialog;
using Game.UI.Animation;

using Scenes.MainMenuScene.Scripts;

using Senspark;

using Services;
using Services.IapAds;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.StoryModeScene.Scripts {
    public class DialogStoryWin : Dialog {
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
        private Text nextText;

        [SerializeField]
        private Text openChestText;
        
        [SerializeField]
        private Button buttonLuckyWheelAds;

        [SerializeField]
        private ButtonWithAlarm buttonLuckyWheelGold;

        [SerializeField]
        private GameObject body;
        
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private ISoundManager _soundManager;
        private IUnityAdsManager _unityAdsManager;
        private IStoryModeManager _storyModeManager;
        private IAnalytics _analytics;
        private IServerManager _serverManager;
        private IServerRequester _serverRequester;
        private IProductItemManager _productItemManager;
        private IInventoryManager _inventoryManager;

        private Action _onNextCallback;
        private Sequence _timeDisplaySeq;
        private string _rewardId;
        private WinReward _goldReward;
        
        private InventoryChestData _chestData;
        private GachaChestItemData[] _itemsReward;
        private IInputManager _inputManager;

        private int _stage;
        private int _level;
        private bool _isChestOpening = false;
        private bool _isSpin = false;

        public static UniTask<DialogStoryWin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStoryWin>();
        }

        protected override void Awake() {
            nextButton.Interactable = false;
            base.Awake();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _unityAdsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            
            var blGold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            buttonLuckyWheelGold.SetGrey(blGold < 100);
        }
        

        protected override void ExtraCheck() {
            if (_inputManager.ReadJoystick(_inputManager.InputConfig.Back) || Input.GetKeyDown(KeyCode.S)) {
                if (!_isSpin && buttonLuckyWheelGold.gameObject.activeInHierarchy && buttonLuckyWheelGold.Interactable) {
                    OnBtLuckyWheel();
                }
            }
        }

        protected override void OnNoClick() {
            // Do nothing
        }

        protected override void OnYesClick() {
            if (!nextButton.gameObject.activeSelf || !nextButton.Interactable) {
                return;
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter) &&
                !_isChestOpening && !_isSpin) {
                OnNextClicked();
            }
        }

        public void SetReward(int stage, int level, string rewardId, IWinReward[] rewards, Action callback) {
            _stage = stage;
            _level = level;
            _rewardId = rewardId;
            _onNextCallback = callback;

            if (rewards.Length == 0) {
                CreateEmptyRewards();
            } else {
                var hadChest = false;
                foreach (var iter in rewards) {
                    var reward = Instantiate(rewardPrefab, rewardContainer, false);
                    var type = RewardResource.ConvertStringToEnum(iter.RewardName);
                    reward.SetInfo(type, iter.Value, iter.OutOfSlot);
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
                }
            }
            if (Application.isMobilePlatform) {
                if (GameConstant.EnableLuckyWheelPve) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(true);
                    buttonLuckyWheelGold.gameObject.SetActive(false);
                    buttonAds.SetActive(false);
                } else if (_goldReward != null && _goldReward.Value > 0) {
                    _analytics.Adventure_TrackSoftCurrencyByWin(_level, _stage, _goldReward.Value);
                    nextButton.SetActive(false);
                    adsButtons.SetActive(true);
                    buttonAds.SetActive(false);
                    buttonLuckyWheelAds.gameObject.SetActive(false);
                    buttonLuckyWheelGold.gameObject.SetActive(false);
                } else {
                    // No Collect Gold
                    adsButtons.SetActive(false);
                    nextButton.SetActive(true);
                }
            } else 
            {
                if (GameConstant.EnableLuckyWheelPve) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(false);
                    buttonLuckyWheelGold.gameObject.SetActive(true);
                    buttonAds.SetActive(false);
                } else {
                    adsButtons.SetActive(false);
                    nextButton.gameObject.SetActive(true);
                }
            }
        }

        private void SetOpenChest() {
            nextText.gameObject.SetActive(false);
            openChestText.gameObject.SetActive(true);
            _chestData = null;
            UniTask.Void(async () => {
                var result = await _inventoryManager.GetChestAsync();
                var chestList = result.ToArray();
                if (chestList.Length == 0) {
                    return;
                }
                _chestData = chestList[0];
                openChestText.text = $"OPEN {_chestData.ChestName.ToUpper()}";
                // Nhằm tránh mất rương vì lý do nào đó user không nhấn nút open chest mà thoát app
                // Tự động mở rương trước sau đó nút open chest chỉ trình diễn animation.
                _itemsReward = await _serverRequester.OpenGachaChest(_productItemManager, _chestData.ChestId);
                TrackOpenChest(_chestData, _itemsReward);
                await _serverManager.General.GetChestReward();
                nextButton.Interactable = true;
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

        private void CreateEmptyRewards() {
            foreach (RewardSourceType iter in Enum.GetValues(typeof(RewardSourceType))) {
                if (iter == RewardSourceType.Rank) {
                    continue;
                }
                var reward = Instantiate(rewardPrefab, rewardContainer, false);
                reward.SetInfo(iter, 0);
            }
        }

        public void OnNextClicked() {
            nextButton.Interactable = false;
            _soundManager.PlaySound(Audio.Tap);

            if (_chestData != null) {
                body.SetActive(false);
                OpenChest();
                return;
            }
            _onNextCallback?.Invoke();
            Hide();
        }

        private async void OpenChest() {
            var dialog =
                await BLDialogGachaChest.CreateFromChestInventory(_itemsReward, _chestData);
            dialog.OnDidHide(() => {
                _onNextCallback?.Invoke();
                Hide();
            });
            dialog.Show(DialogCanvas);

        }
        
        public void OnAdsClicked() {
            
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
            _isSpin = true;
            BLDialogLuckyWheel.Create().ContinueWith(dialog => {
                dialog.SetData(DialogCanvas, adsButtons, _rewardId, false, "win",
                    () => {
                        adsButtons.SetActive(false);
                        nextButton.Interactable = true;
                        _isSpin = false;
                    },
                    _level, _stage);
                dialog.Show(DialogCanvas);
            });
        }
    }
}