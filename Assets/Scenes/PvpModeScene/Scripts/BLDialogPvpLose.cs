using App;

using BLPvpMode.Engine.Info;
using BLPvpMode.UI;

using BomberLand.Button;
using BomberLand.Component;

using Constant;

using Cysharp.Threading.Tasks;

using Engine.Input;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI.Animation;

using PvpMode.Manager;

using Scenes.StoryModeScene.Scripts;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class BLDialogPvpLose : Dialog {
        [SerializeField]
        private GameObject drawContent;

        [SerializeField]
        private GameObject loseContent;

        [SerializeField]
        private Transform rewardContainer;

        [SerializeField]
        private WinReward rewardPrefab;

        [SerializeField]
        private GameObject adsButtons;

        [SerializeField]
        private ButtonZoomAndFlash nextButton;

        [SerializeField]
        private Button smallNextButton;

        [SerializeField]
        private Button buttonLuckyWheelAds;

        [SerializeField]
        private ButtonWithAlarm buttonLuckyWheelGold;

        [SerializeField]
        private GameObject loseInfo;

        [SerializeField]
        private GameObject score;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private GameObject itemLoot;

        [SerializeField]
        private GameObject boosterContent;

        [SerializeField]
        private Image boosterIcon;

        [SerializeField]
        private TextMeshProUGUI boosterText;

        [SerializeField]
        private BLGachaRes resource;

        private System.Action _onClaimCallback;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private IInputManager _inputManager;
        private string _pvpRewardId;
        private bool _isSpinLuckyWheel;

        public static UniTask<BLDialogPvpLose> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvpLose>();
        }

        protected override void Awake() {
            base.Awake();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();

            var blGold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            buttonLuckyWheelGold.SetGrey(blGold < 100);
        }

        protected override void ExtraCheck() {
            if (_inputManager.ReadJoystick(_inputManager.InputConfig.Back) || Input.GetKeyDown(KeyCode.S)) {
                if (buttonLuckyWheelGold.Interactable && buttonLuckyWheelGold.gameObject.activeInHierarchy) {
                    OnBtLuckyWheel();
                }
            }
        }

        protected override void OnYesClick() {
            if (!nextButton.gameObject.activeSelf || !nextButton.Interactable) {
                return;
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)
                || !_isSpinLuckyWheel) {
                OnNextButtonClicked();
            }

        }

        protected override void OnNoClick() {
            //do nothing
        }

        public void SetRewards(
            LevelResult levelResult,
            IPvpResultUserInfo userInfo,
            string rewardId,
            bool isOutOfChest,
            System.Action callback
        ) {
            loseInfo.SetActive(true);
            score.SetActive(false);

            if (levelResult == LevelResult.Draw) {
                drawContent.SetActive(true);
                loseContent.SetActive(false);
            } else {
                drawContent.SetActive(false);
                loseContent.SetActive(true);
            }

            _onClaimCallback = callback;
            _pvpRewardId = rewardId;
            CreateRewardItem(RewardSourceType.Rank, userInfo.DeltaPoint, false);
            foreach (var iter in userInfo.Rewards.Keys) {
                CreateRewardItem(RewardResource.ConvertIdToEnum(iter),
                    (int)userInfo.Rewards[iter],
                    isOutOfChest);
            }

            if (Application.isMobilePlatform) {
                if (GameConstant.EnableLuckyWheelPvp) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(true);
                    buttonLuckyWheelGold.gameObject.SetActive(false);
                } else {
                    adsButtons.SetActive(false);
                    nextButton.SetActive(true);
                }
            } else {
                if (GameConstant.EnableLuckyWheelPvp) {
                    adsButtons.SetActive(true);
                    buttonLuckyWheelAds.gameObject.SetActive(false);
                    buttonLuckyWheelGold.gameObject.SetActive(true);
                } else {
                    adsButtons.SetActive(false);
                    nextButton.SetActive(true);
                }
            }
        }

        public async void UpdateBooster(int[] boosterIds) {
            boosterContent.SetActive(false);
            foreach (var boosterId in boosterIds) {
                var type = DefaultBoosterManager.ConvertFromId(boosterId);
                switch (type) {
                    case BoosterType.RankGuardian:
                        boosterIcon.sprite = await resource.GetSpriteByItemId(boosterId);
                        boosterText.text = "-50%";
                        boosterContent.SetActive(true);
                        return;
                    case BoosterType.FullRankGuardian:
                        boosterIcon.sprite = await resource.GetSpriteByItemId(boosterId);
                        boosterText.text = "-100%";
                        boosterContent.SetActive(true);
                        return;
                }
            }
        }

        public void SetTournamentResult(
            LevelResult levelResult,
            IPvpResultInfo info,
            int slot,
            System.Action callback
        ) {
            Assert.IsTrue(info.Scores.Length == 2);
            loseInfo.SetActive(false);
            score.SetActive(true);
            itemLoot.SetActive(false);

            if (levelResult == LevelResult.Draw) {
                drawContent.SetActive(true);
                loseContent.SetActive(false);
            } else {
                drawContent.SetActive(false);
                loseContent.SetActive(true);
            }

            _onClaimCallback = callback;

            scoreText.text = $"{info.Scores[slot]}-{info.Scores[1 - slot]}";
            nextButton.gameObject.SetActive(true);
            adsButtons.SetActive(false);
        }

        private void CreateRewardItem(RewardSourceType type, int value, bool outOfSlot) {
            var item = Instantiate(rewardPrefab, rewardContainer, false);
            var fullSlot = RewardIsChest(type) && outOfSlot;
            item.SetInfo(type, value, fullSlot);
        }

        private bool RewardIsChest(RewardSourceType type) {
            return type switch {
                RewardSourceType.Rank => false,
                RewardSourceType.Gold => false,
                _ => true
            };
        }

        public void OnNextButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            _onClaimCallback?.Invoke();
            Hide();
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