using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using App;
using BLPvpMode.UI;
using BomberLand.Component;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Engine.Input;
using Engine.Utils;
using Game.Dialog;
using Game.Manager;
using Game.UI;
using GroupMainMenu;
using PvpMode.Manager;
using Reconnect;
using Reconnect.Backend;
using Reconnect.View;
using Scenes.MainMenuScene.Scripts.Controller;
using Scenes.PvpModeScene.Scripts;
using Scenes.StoryModeScene.Scripts;
using Senspark;
using Services;
using Share.Scripts.Dialog;
using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public enum CurrentStageInMainMenu {
        None,
        ShowInfo,
        FindMatch,
        ShowSetting,
    }

    public class GuiMainMenu : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Transform topLeft;

        [SerializeField]
        private Transform topRight;

        [SerializeField]
        private Transform left;

        [SerializeField]
        private Transform center;

        [SerializeField]
        private Transform right;

        [SerializeField]
        private Transform bottomLeft;

        [SerializeField]
        private Transform bottomCenter;

        [SerializeField]
        private Transform bottomRight;

        [SerializeField]
        private Transform pack;

        [SerializeField]
        private GuiMainMenuResource resource;

        [SerializeField]
        private BLTutorialGui tutorialGui;

        private MMHeroSelector _heroSelector;
        private MMHeroChoose _heroChoose;
        private MMChangeMode _changeMode;
        private MMButton _buttonPlay;
        private MMButton _findMatch;
        private MMButton _buttonSetting;

        private BLProfileCard _profileCard;
        private MMWarningPointDecay _warningPointDecay;
        private MMHeaderBar _headerBar;
        private BLDailyRewardButton _redDotDailyButton;
        private BLRedDotInventoryButton _redDotInvButton;
        private MMRankButton _rankButton;
        private MMDailyTaskButton _dailyTaskButton;
        private BLDialogIapPackIcons _starterPack;
        private MMDailyTaskNoti _dailyTaskNoti;

        private List<MainMenuButton> _mainMenuButtons = new List<MainMenuButton>();
        private List<MMButton> _buttons = new List<MMButton>();

        private IReconnectStrategy _reconnectStrategy;
        private MainMenuController _mainMenuController;
        private FindMatchController _findMatchController;
        private ILogManager _logManager;
        private IInputManager _inputManager;
        private IAnalytics _analytics;
        private IServerManager _serverManager;
        private IServerRequester _serverRequester;
        private IProductItemManager _productItemManager;
        private IInventoryManager _inventoryManager;
        private IPvPBombRankManager _rankManager;
        private IDailyTaskManager _dailyTaskManager;

        private bool _isReady;
        private ObserverHandle _handle;
        private WaitingUiManager _waiting;
        private bool _initialized;

        private CurrentStageInMainMenu _currentStage = CurrentStageInMainMenu.None;

        private void Awake() {
            Initialized().Forget();
        }

        public UniTask Initialized() {
            if (_initialized) {
                return UniTask.CompletedTask;
            }

            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _reconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _mainMenuController = new MainMenuController();
            _findMatchController = new FindMatchController();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _rankManager = ServiceLocator.Instance.Resolve<IPvPBombRankManager>();
            _dailyTaskManager = ServiceLocator.Instance.Resolve<IDailyTaskManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_dailyTaskManager, new DailyTaskObserver() {
                updateHeroChoose = InitHeroChoose
            });

            _initialized = true;
            return UniTask.CompletedTask;
        }

        public void CreateSkeletonElements() {
            CreateTopLeftElements();
            CreateTopRightElements();
            CreateCenterElements();
            CreateLeftElements();
            CreateRightElements();
            CreateBottomLeftElements();
            CreateBottomCenterElements();
            CreateBottomRightElements();
            CreatePackElement();
        }

        public void InitControllers(IEnumerable<NewcomerGiftData> newcomerGift) {
            _mainMenuController.Initialized(_reconnectStrategy,
                UpdateLatency,
                ShowDialogConfirmUpgrade,
                ShowDialogForceUpgrade);

            _findMatchController.Initialized(
                ShowCancelButton,
                StopFinding,
                UpdateCountDownText, ShowError);

            _mainMenuController.CheckRedDot(_redDotDailyButton, _redDotInvButton);
            _dailyTaskButton.SetCanvasDialog(canvasDialog);
            _dailyTaskButton.LoadData();
            _rankButton.SetCanvasDialog(canvasDialog);
            if (AppConfig.IsTournament()) {
                HideButtonsForTournament();
            } else {
                _rankButton.LoadData();
            }
            ShowDialogReward(newcomerGift);
            _isReady = true;
        }

        private async void ShowDialogConfirmUpgrade() {
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                _mainMenuController.GetString(Application.isMobilePlatform
                    ? LocalizeKey.ui_request_update_game_mobile
                    : LocalizeKey.ui_request_update_game_webgl),
                "Yes",
                "No",
                App.Utils.GoToStore,
                () => { }
            );
            confirm.Show(canvasDialog);
        }

        private void ShowDialogForceUpgrade() {
            var msg = _mainMenuController.GetString(Application.isMobilePlatform
                ? LocalizeKey.ui_request_update_game_mobile
                : LocalizeKey.ui_request_update_game_webgl);
            DialogOK.ShowInfoAsync(canvasDialog, msg, new DialogOK.Optional {
                OnWillHide = () => {
                    App.Utils.GoToStore();
                    Application.Quit();
                }
            }).Forget();
        }

        private void Update() {
            if (!_isReady) {
                return;
            }

            if (canvasDialog.transform.childCount > 0) {
                return;
            }

            if (Input.GetKeyDown(KeyBoardInputDefine.FindMatch) ||
                _inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                if (_currentStage == CurrentStageInMainMenu.None) {
                    OnPlayClick();
                    return;
                }
            }

            //Đang tìm trận thì mới đc ấn nút back để hủy
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                if (_currentStage == CurrentStageInMainMenu.FindMatch) {
                    OnCancel();
                    return;
                }
            }

            // Mở dialog equip
            if (Input.GetKeyDown(KeyCode.E)
                || _inputManager.ReadButton(ControllerButtonName.X)) {
                if (_currentStage == CurrentStageInMainMenu.None) {
                    OnEquipClicked();
                    return;
                }
            }

            // Tắt dialog equip
            if (_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                if (_currentStage == CurrentStageInMainMenu.ShowInfo) {
                    OnAcceptClicked();
                    return;
                }
            }

            if (AppConfig.IsTournament()) {
                return;
            }
            if (_inputManager.ReadButton(_inputManager.InputConfig.Settings)) {
                if (_currentStage == CurrentStageInMainMenu.None) {
                    OnSettingClicked();
                    return;
                }
            }

            var delta = Time.deltaTime;
            _findMatchController.OnProcess(delta);
        }

        private void OnPlayClick() {
            if (_buttonPlay.gameObject.activeInHierarchy) {
                OnPlayClicked();
            }
        }

        private void OnCancel() {
            if (!_buttonPlay.gameObject.activeInHierarchy) {
                OnCancelFindClicked();
            }
        }

        private void OnDestroy() {
            _handle.Dispose();
            _mainMenuController.OnDestroy();
            _findMatchController.OnDestroy();
        }

        private void ShowDialogReward(IEnumerable<NewcomerGiftData> newcomerGift) {
            if (newcomerGift == null) {
                return;
            }
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            BLDialogLuckyWheelReward.CreateDialogTutorialClaimLarge().ContinueWith(dialogReward => {
                dialogReward.UpdateUI(newcomerGift.ToList().Select(it => (it.ItemId, it.Quantity)));
                dialogReward.OnDidHide(() => { soundManager.PlaySound(Audio.Tap); });
                dialogReward.Show(canvasDialog);
                soundManager.PlaySound(Audio.TutorialReward);
            });
        }

        private void HideButtonsForTournament() {
            // Khóa tất cả các nút
            _headerBar.SetInteractable(false);
            _starterPack.SetInteractable(false);
            SetButtonsInteractable(false);
            // Ngoại trừ nút Setting và HeroChoose
            _buttonSetting.Interactable = true;
            _heroChoose.Button.Interactable = true;
        }

        public void ForceNewUserPlayPvp() {
            var buttonPlayGroup = _buttonPlay.transform.parent;
            var content = buttonPlayGroup.parent;
            var instructionsTap = tutorialGui.CreateInstructionsTap(content);
            instructionsTap.HideWelcome();
            instructionsTap.Show();
            var pos = buttonPlayGroup.transform.localPosition;
            instructionsTap.SetSystemText("TAP TO PLAY", pos + new Vector3(-120, 220, 0));
            buttonPlayGroup.SetParent(instructionsTap.transform);
            var pointerHand = tutorialGui.CreateHandTouchOnUi(content.transform);
            pointerHand.transform.localRotation = Quaternion.Euler(0, 0, 0);
            pointerHand.transform.localPosition = pos + new Vector3(-120, 80, 0);
            _buttonPlay.Button.onClick.AddListener(() => {
                instructionsTap.HideSystemBox();
                pointerHand.gameObject.SetActive(false);
            });
        }

        public void HideStarterPack() {
            _starterPack.gameObject.SetActive(false);
        }

        public void CheckAndShowOffers() {
            _starterPack.CheckAndShowOffers();
        }

        private void CreateTopLeftElements() {
            ScaleForPad(topLeft.gameObject);
            _profileCard = resource.CreateProfileCared(topLeft);
            _profileCard.SetBtnClicked(ShowDialogProfile); // ShowInventory(BLTabType.Avatar));
        }

        public void TryLoadProFileCard() {
            _profileCard.TryLoadData();
        }

        private void CreateTopRightElements() {
            var ratio = ScreenUtils.GetScreenRatio();
            if (!ScreenUtils.IsIPadScreen()) {
                _headerBar = resource.CreateHeaderBar(topRight, OnTokenClicked);
            } else {
                _headerBar = resource.CreateIpadHeaderBar(topRight, OnTokenClicked);
            }
            _buttonSetting = resource.CreateSettingButton(topRight, OnSettingClicked);
            _buttons.Add(_buttonSetting.GetComponent<MMButton>());
        }

        private void CreateLeftElements() {
            ScaleForPad(left.gameObject);
            //Tạm ẩn nút Quest
            //var leftButtons = new ButtonType[] { ButtonType.Market, ButtonType.TreasureHunt, ButtonType.Quest, };
            var leftButtons = new ButtonType[] { ButtonType.Market, ButtonType.TreasureHunt };
            foreach (var buttonType in leftButtons) {
                var button = resource.CreateGreenButton(buttonType, left, OnMenuButtonClicked);
                // if (buttonType == ButtonType.TreasureHunt) {
                //     button.gameObject.SetActive(GameConstant.FiPlatform);
                // }
                _mainMenuButtons.Add(button.GetComponent<MainMenuButton>());
            }
        }

        private void CreateCenterElements() {
            var heroChooseCallback = new HeroChooseCallback() {
                OnShowDialog = OnShowDialog,
                OnEquipClicked = OnEquipClicked,
                OpenAllChests = OpenAllChests
            };
            _heroChoose = resource.CreateHero(center, heroChooseCallback);
            ScaleForPad(_heroChoose.gameObject, 0.8f);

            _heroChoose.SetCanvas(canvasDialog);
            _buttons.Add(_heroChoose.Button);

            _heroSelector = resource.CreateHeroSelector(center);
            _heroSelector.gameObject.SetActive(false);

            _dailyTaskNoti = resource.CreateDailyTaskNoti(center, canvasDialog);
            _dailyTaskNoti.SetCanvas(canvasDialog);
        }

        public void InitHeroChoose() {
            _heroChoose.InitHeroChoose();
            _heroSelector.Initialized(canvasDialog,
                OnHeroSelected,
                OnWingEquipped,
                OnBombEquipped,
                OnHeroClicked,
                OnNotificationToMarket,
                OnAcceptClicked);
            _mainMenuController.SaveBoosterStatus(_heroSelector.GetBoosterStatus());
        }

        private void CreateRightElements() {
            ScaleForPad(right.gameObject);
            var rightButtons = new ButtonType[] { ButtonType.Shop, ButtonType.Rank, ButtonType.DailyTask, };
            foreach (var buttonType in rightButtons) {
                var button = resource.CreateGreenButton(buttonType, right, OnMenuButtonClicked);
                switch (buttonType) {
                    case ButtonType.DailyGift:
                        button.gameObject.SetActive(GameConstant.MobilePlatform);
                        _redDotDailyButton = button.GetComponent<BLDailyRewardButton>();
                        break;
                    case ButtonType.Rank:
                        _rankButton = button.GetComponent<MMRankButton>();
                        break;
                    case ButtonType.DailyTask:
                        _dailyTaskButton = button.GetComponent<MMDailyTaskButton>();
                        break;
                }
                _mainMenuButtons.Add(button.GetComponent<MainMenuButton>());
            }
        }

        private void CreateBottomLeftElements() {
            ScaleForPad(bottomLeft.gameObject);
            var bottomLeftButtons = new ButtonType[] { ButtonType.Altar, ButtonType.Inv, ButtonType.Hero, };
            foreach (var buttonType in bottomLeftButtons) {
                var button = resource.CreateBrownButton(buttonType, bottomLeft, OnMenuButtonClicked);
                if (buttonType == ButtonType.Inv) {
                    _redDotInvButton = button.GetComponent<BLRedDotInventoryButton>();
                }
                _mainMenuButtons.Add(button.GetComponent<MainMenuButton>());
            }
        }

        private void CreateBottomCenterElements() {
            // do nothing
        }

        public void InitPointDecay() {
            UniTask.Void(async () => {
                await _rankManager.InitializeAsync();
                var currentRank = _rankManager.GetBombRank();
                var remainingMatches =
                    Mathf.Max(_rankManager.GetMinMatchesConfig() - _rankManager.GetAmountMatches(), 0);
                var localNow = DateTime.UtcNow.ToLocalTime();
                // Check if local time after 16:00
                if (localNow.Hour >= 16 && remainingMatches > 0 && currentRank >= PvpRankType.Copper1) {
                    _warningPointDecay = resource.CreateWarningPointDecay(_heroChoose.GetComponent<Transform>());
                    _warningPointDecay.SetCanvasDialog(canvasDialog);
                }
            });
        }

        private void CreateBottomRightElements() {
            ScaleForPad(bottomRight.gameObject, 0.8f);
            if (AppConfig.IsTournament()) {
                resource.CreateTournamentMode(bottomRight);
                _buttonPlay = resource.CreateButtonPlay(bottomRight, OnPlayClicked);
            } else {
                _changeMode = resource.CreateChangeMode(bottomRight);
                _buttonPlay = resource.CreateButtonPlay(bottomRight, OnPlayClicked);
                _findMatch = resource.CreateFindMatch(bottomRight, OnCancelFindClicked);
                _findMatch.gameObject.SetActive(false);
            }
            var animationZoom = _buttonPlay.GetComponent<AnimationZoom>();
            if (animationZoom != null) {
                UniTask.Void(async () => {
                    animationZoom.Stop();
                    await UniTask.Delay(500);
                    animationZoom.Stop();
                    animationZoom.Play();
                });
            }
        }

        private void CreatePackElement() {
            _starterPack = resource.CreateStarterPack(pack);
            _starterPack.SetCanvasDialog(canvasDialog);
        }

        private void ScaleForPad(GameObject obj, float scale = 0.9f) {
            if (!ScreenUtils.IsIPadScreen()) {
                return;
            }
            var trans = obj.GetComponent<Transform>();
            var localScale = trans.localScale;
            localScale *= scale;
            trans.localScale = localScale;
        }

        private void OnTokenClicked(BlockRewardType token) {
            _mainMenuController.PlaySoundTap();
            var canUseShop = true;
            if (!Application.isMobilePlatform) {
                canUseShop = token == BlockRewardType.BLGold;
            }
            if (canUseShop) {
                var typeMenuLeft = token switch {
                    BlockRewardType.BLGold => TypeMenuLeftShop.Gold,
                    BlockRewardType.Gem => TypeMenuLeftShop.Gems,
                    BlockRewardType.LockedGem => TypeMenuLeftShop.Gems,
                    _ => TypeMenuLeftShop.Chest,
                };
                if (typeMenuLeft != TypeMenuLeftShop.Chest) {
                    ShopScene.Scripts.ShopScene.LoadScene(typeMenuLeft);
                }
                return;
            }

            DialogOK.ShowInfo(canvasDialog, "MOBILE ONLY",
                "Gems purchase only available on\nBomb Crypto (BCOIN) mobile");
        }

        private bool _settingClicked;

        private void OnSettingClicked() {
            if (_settingClicked)
                return;
            _settingClicked = true;
            _currentStage = CurrentStageInMainMenu.ShowSetting;
            _mainMenuController.PlaySoundTap();
            _ = DialogSetting.Create().ContinueWith(dialog => {
                dialog.Show(canvasDialog);
                dialog.OnDidHide(() => {
                    _settingClicked = false;
                    _currentStage = CurrentStageInMainMenu.None;
                });
            });
        }

        private void OnMenuButtonClicked(ButtonType buttonType) {
            _mainMenuController.PlaySoundTap();
            switch (buttonType) {
                case ButtonType.Market:
                    ShowMarket();
                    break;
                case ButtonType.Quest:
                    ShowQuest();
                    break;
                case ButtonType.Shop:
                    ShowShop();
                    break;
                case ButtonType.DailyGift:
                    _mainMenuController.ShowDailyGift();
                    break;
                case ButtonType.Rank:
                    ShowRank();
                    break;
                case ButtonType.DailyTask:
                    ShowDailyTask();
                    break;
                case ButtonType.Altar:
                    _mainMenuController.ShowAltar();
                    break;
                case ButtonType.Inv:
                    ShowInventory();
                    break;
                case ButtonType.Hero:
                    _mainMenuController.ShowHero();
                    break;
                case ButtonType.TreasureHunt:
                    ShowTreasureHunt();
                    break;
            }
        }

        private void ShowMarket() {
            if (_mainMenuController.IsDisableFeature(FeatureId.Marketplace)) {
                DialogOK.ShowError(canvasDialog, _mainMenuController.GetString(LocalizeKey.ui_feature_maintenance));
                return;
            }
            _mainMenuController.TrackConversionClickMarket();
            _mainMenuController.ShowMarket();
        }

        private void ShowQuest() {
        }

        private void ShowTreasureHunt() {
            if (_mainMenuController.IsDisableFeature(FeatureId.TreasureHunt)) {
                DialogOK.ShowInfo(canvasDialog, _mainMenuController.GetString(LocalizeKey.ui_feature_maintenance));
                return;
            }
            if (!_mainMenuController.IsEnableTreasureHunt()) {
                DialogIntroduction.ShowInfo(canvasDialog);
                return;
            }
            _mainMenuController.PlaySoundTap();
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();

            var suitable = _mainMenuController.IsEnableBHero();
            if (suitable && DialogAmazonWarning.CanShow()) {
                var warning = DialogAmazonWarning.Create();
                warning.Init(() => { OpenMode().Forget(); });
                warning.Show(canvasDialog);
            } else {
                OpenMode().Forget();
            }
            return;

            async UniTask OpenMode() {
                try {
                    await _mainMenuController.ShowTreasureHunt();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    DialogError.ShowError(canvasDialog, "Loading Problems, please try again after a few seconds.");
                    waiting.End();
                }
            }
        }

        private void ShowShop() {
            if (_mainMenuController.IsDisableFeature(FeatureId.Marketplace)) {
                DialogOK.ShowError(canvasDialog, _mainMenuController.GetString(LocalizeKey.ui_feature_maintenance));
                return;
            }
            _mainMenuController.ShowShop();
        }

        private void ShowRank() {
            _rankButton.ShowPvpRanking();
        }

        private void ShowDailyTask() {
            _dailyTaskButton.ShowDailyTask();
        }

        private void ShowDialogProfile() {
            DialogProfile.Create().ContinueWith(dialog => {
                dialog.InitCurrentHero(() => ShowInventory(BLTabType.Avatar));
                dialog.Show(canvasDialog);
            });
        }

        private void ShowInventory(BLTabType blTabType = BLTabType.Heroes) {
            if (_mainMenuController.IsDisableFeature(FeatureId.Inventory)) {
                DialogOK.ShowError(canvasDialog, _mainMenuController.GetString(LocalizeKey.ui_feature_maintenance));
                return;
            }
            _mainMenuController.ShowInv(blTabType);
        }

        private void OnShowDialog(Dialog dialog) {
            dialog.OnDidHide(OpenAllChests);
            dialog.Show(canvasDialog);
        }

        private void OpenAllChests() {
            UniTask.Void(async () => {
                var result = await _inventoryManager.GetChestAsync();
                var chestList = result.ToArray();
                foreach (var chestData in chestList) {
                    if (chestData.ChestId == 0) {
                        continue;
                    }
                    await OpenChest(chestData);
                }
            });
        }

        private async Task OpenChest(InventoryChestData chestData) {
            var itemsReward = await _serverRequester.OpenGachaChest(_productItemManager, chestData.ChestId);
            TrackOpenChest(chestData, itemsReward);
            await _serverManager.General.GetChestReward();
            var dialog = await BLDialogGachaChest.CreateFromChestInventory(itemsReward, chestData);
            dialog.Show(canvasDialog);
            await dialog.WaitForHide();
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

        private void OnEquipClicked() {
            _currentStage = CurrentStageInMainMenu.ShowInfo;
            _mainMenuController.PlaySoundTap();
            _heroSelector.gameObject.SetActive(true);
        }

        private void OnHeroSelected(int heroId) {
            _mainMenuController.PlaySoundTap();
            _heroChoose.UpdateHero(heroId);
        }

        private void OnWingEquipped(int wingId) {
            _mainMenuController.PlaySoundTap();
            _heroChoose.UpdateWing(wingId);
        }

        private void OnBombEquipped(int bombId) {
            _mainMenuController.PlaySoundTap();
            _heroChoose.UpdateBomb(bombId);
        }

        private void OnHeroClicked(HeroId heroId,
            List<HeroGroupData> pvpHeroes,
            Dictionary<int, StatData[]> skinStats) {
            _mainMenuController.PlaySoundTap();
            DialogHeroSelection.Create().ContinueWith(dialog => {
                dialog.SetInfo(heroId, pvpHeroes, skinStats, OnHeroSelected);
                dialog.Show(canvasDialog);
            });
        }

        private void OnHeroSelected(HeroId pvpHeroId) {
            _mainMenuController.PlaySoundTap();
            _heroSelector.UpdateHeroChoose(pvpHeroId.Id);
        }

        private void OnNotificationToMarket(BoosterType boosterType, Action callback) {
            DialogNotificationBoosterToMarket.ShowOn(canvasDialog, boosterType, callback);
        }

        private void OnAcceptClicked() {
            _currentStage = CurrentStageInMainMenu.None;
            _mainMenuController.PlaySoundTap();
            _mainMenuController.SaveBoosterStatus(_heroSelector.GetBoosterStatus());
            _heroSelector.gameObject.SetActive(false);
        }

        private void OnPlayClicked() {
            _mainMenuController.PlaySoundTap();
            if (AppConfig.IsTournament()) {
                DialogPvpSchedule.Create().ContinueWith(dialog => { dialog.Show(canvasDialog); });
            } else {
                switch (_changeMode.GameMode) {
                    case GameModeType.StoryMode:
                        PlayStoryMode();
                        break;
                    case GameModeType.PvpMode:
                        PlayPvpMode();
                        break;
                }
            }
        }

        private void PlayStoryMode() {
            _mainMenuController.PlaySoundTap();
            if (_mainMenuController.IsDisableFeature(FeatureId.PvE)) {
                DialogOK.ShowError(canvasDialog, _mainMenuController.GetString(LocalizeKey.ui_feature_maintenance));
                return;
            }
            if (!_mainMenuController.IsEnableStoryMode()) {
                DialogOK.ShowError(canvasDialog, "Story Mode Not Enabled in Feature");
                return;
            }
            _mainMenuController.PlayStoryMode();
        }

        private void OnCancelFindClicked() {
            _currentStage = CurrentStageInMainMenu.None;
            _findMatch.SetInteractable(false);
            _mainMenuController.PlaySoundTap();
            _findMatchController.CancelFinding();
        }

        #region Find Match PVP

        private void PlayPvpMode() {
            _currentStage = CurrentStageInMainMenu.FindMatch;
            _mainMenuController.PlaySoundTap();
            ShowFinding();
            _findMatchController.StartFindMatch();
        }

        private void UpdateLatency(int lag) {
            _profileCard.UpdateLatency(lag);
        }

        private void ShowCancelButton() {
            _findMatch.SetInteractable(true);
            _findMatch.SetVisible(true);
        }

        private void ShowFinding() {
            _headerBar.SetInteractable(false);
            _starterPack.SetInteractable(false);
            SetButtonsInteractable(false);

            _buttonPlay.gameObject.SetActive(false);
            _findMatch.SetVisible(false);
            _findMatch.gameObject.SetActive(true);
        }

        private void StopFinding() {
            _headerBar.SetInteractable(true);
            _starterPack.SetInteractable(true);
            SetButtonsInteractable(true);

            _findMatch.gameObject.SetActive(false);
            _buttonPlay.gameObject.SetActive(true);
        }

        private void SetButtonsInteractable(bool value) {
            _profileCard.SetEnable(value);
            if (_changeMode) {
                _changeMode.Interactable = value;
            }
            foreach (var button in _mainMenuButtons) {
                button.Interactable = value;
            }
            foreach (var button in _buttons) {
                button.Interactable = value;
            }
        }

        private void UpdateCountDownText(string text) {
            _findMatch.SetCounter(text);
        }

        private void ShowError(string message) {
            DialogOK.ShowError(canvasDialog, message, () => { _currentStage = CurrentStageInMainMenu.None; });
        }

        public void OpenEquip() {
            OnEquipClicked();
        }

        public void OpenProfile() {
            ShowDialogProfile();
        }

        #endregion
    }
}