using System;
using System.Threading.Tasks;

using App;

using BLPvpMode.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.UI;

using BomberLand.Component;
using BomberLand.InGame;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Camera;
using Engine.Entities;
using Engine.Manager;
using Engine.Utils;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI;

using PvpMode.Dialogs;
using PvpMode.Manager;
using PvpMode.Utils;

using Scenes.StoryModeScene.Scripts;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class BLGuiPvp : MonoObserverManager<BLGuiObserver>, IBLGui, IBLParticipantGui {
        [SerializeField]
        private GameObject[] stats;

        [SerializeField]
        private GameObject heroGroup;

        [SerializeField]
        private GameObject matchId;

        [SerializeField]
        private Text matchIdText;

        [SerializeField]
        private GameObject timer;

        [SerializeField]
        private Text timerText;

        [SerializeField]
        private Avatar[] heroes;
        
        [SerializeField]
        private ImageAnimation[] avatarTRs;
        
        [SerializeField]
        private EmojiInPlayer[] emojis;
        
        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Text[] addressText;

        [SerializeField]
        private Text[] numItems;

        [SerializeField]
        private Text mainLatencyText;

        [SerializeField]
        private Text[] latencyText;

        [SerializeField]
        private Color[] latencyColors;

        [SerializeField]
        private BLHealthUI healthUI;

        [SerializeField]
        private Text[] healthText;

        #region Flying reward

        [SerializeField]
        private Transform iconChest;

        [SerializeField]
        private BLFlyingReward flyingReward;

        #endregion

        #region Emoji

        [SerializeField]
        private EmojiBar emojiBar;

        [SerializeField]
        private EmojiIcon emojiPrefab;

        #endregion

        private BLAdjustPvPMapUI _adjustUi;
        private Camera _camera;
        private ISoundManager _soundManager;
        private BoosterStatus _boosterStatus;
        private ButtonUseBooster _boosterButtonRequest;
        private DialogHurryUp _hurryUp;
        public IBLInputKey Input { get; private set; }

        public void Initialized(IBLInputKey input) {
            Input = input;
        }

        private void Awake() {
            _camera = Camera.main;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _adjustUi = GetComponent<BLAdjustPvPMapUI>();
            _hurryUp = DialogHurryUp.Create(transform);
        }

        public void HideTimer() {
            timer.SetActive(false);
        }

        public ProCamera SetUpCamera(int col, int row) {
            return _adjustUi.SetUpCamera(col, row);
        }

        public async void UpdatePvpInfo(
            string matchId,
            string serverId,
            int slot,
            IMatchUserInfo[] pvpUsers,
            BoosterStatus boosterStatus) {
            var isParticipant = slot < pvpUsers.Length;
            var shortenMatchId = matchId.Length > 10 ? $"{matchId[..5]}...{matchId[^5..]}" : matchId;
            var id = serverId == null ? shortenMatchId : $"{serverId}:{shortenMatchId}";
            matchIdText.text = $"ID:{id}";
            heroes[0].ChangeImage(DefaultPlayerManager.TrPlayerType(pvpUsers[0].Hero.Skin), PlayerColor.HeroTr);
            heroes[1].ChangeImage(DefaultPlayerManager.TrPlayerType(pvpUsers[1].Hero.Skin), PlayerColor.HeroTr);
            
            var sprites0 = await resource.GetAvatar(pvpUsers[0].Avatar);
            avatarTRs[0].StartAni(sprites0);
            var sprites1 = await resource.GetAvatar(pvpUsers[1].Avatar);
            avatarTRs[1].StartAni(sprites1);

            SetUserName(addressText[0], pvpUsers[0]);
            SetUserName(addressText[1], pvpUsers[1]);
            for (var i = 0; i < addressText.Length; i++) {
                if (isParticipant) {
                    if (i == slot) {
                        iconChest = heroes[i].transform;
                        addressText[i].color = Color.white;
                    } else {
                        addressText[i].color = new Color(0.9607f, 0.2588f, 0.1568f, 1f);
                    }
                } else {
                    addressText[i].color = Color.white;
                }
            }

            _boosterStatus = boosterStatus;
            var inputCallback = new PlayerInputCallback {
                OnBombButtonClicked = OnBombButtonClicked,
                OnBoosterButtonClicked = OnBoosterButtonClicked,
                OnBackButtonClicked = OnBackButtonClicked
            };
            Input.SetupButtons(_boosterStatus, inputCallback);
        }

        public void UpdateBoosters(BoosterStatus boosterStatus, float coolDown) {
            _boosterStatus = boosterStatus;
            Input.UpdateButtons(_boosterStatus, coolDown);
        }

        public void InitEmojiButtons(int[] itemIds) {
            Input.InitEmojiButtons(itemIds, OnEmojiClicked);
        }
        
        public void ShowEmojiUi(int slot, int itemId) {
            if (Input.GetMuteState()) {
                return;
            }
            emojis[slot].SetAnimation(itemId, () => emojis[slot].SetActive(false));
            emojis[slot].SetActive(true);
        }

        public async Task ShowDialogError(Canvas canvasDialog, string message) {
            await DialogOK.ShowErrorAsync(canvasDialog, message, new DialogOK.Optional { WaitUntilHidden = true });
        }

        public void ShowErrorAndKick(Canvas canvasDialog, string reason) {
            DialogOK.ShowError(canvasDialog, reason, () => {
                // Tạm để PVP thoát ra MainMenu
                const string sceneName = "MainMenuScene";
                SceneLoader.LoadSceneAsync(sceneName).Forget();
            });
        }

        public IBLParticipantGui GetParticipantGui() {
            return this;
        }

        public void CheckInputKeyDown() {
            Input.CheckInputKeyDown();
        }

        public Vector2 GetDirectionFromInput() {
            return Input.GetDirection();
        }

        private void SetUserName(Text text, IMatchUserInfo user) {
            string userName;
            if (string.IsNullOrEmpty(user.DisplayName)) {
                userName = Ellipsis.EllipsisAddress(App.Utils.GetShortenName(user.Username));
            } else {
                userName = Ellipsis.EllipsisAddress(App.Utils.GetShortenName(user.DisplayName));
            }
            text.text = userName;
        }

        private void OnEmojiClicked(int itemId) {
            DispatchEvent(e => e.UseEmoji(itemId));
        }

        private void OnBombButtonClicked() {
            DispatchEvent(e => e.SpawnBomb());
        }

        private void OnBoosterButtonClicked(BoosterType type) {
            DispatchEvent(e => e.UseBooster(type));
        }

        public void OnBackButtonClicked() {
            DispatchEvent(e => e.RequestQuit());
        }

        public void ShowDialogQuit(Canvas canvasDialog, Action onQuickCallback, Action onHideCallback) {
            _soundManager.PlaySound(Audio.Tap);
            DialogPvpQuit.Create().ContinueWith(dialog => {
                dialog.OnQuitCallback = onQuickCallback;
                dialog.OnDidHide(onHideCallback.Invoke);
                dialog.Show(canvasDialog);
            });
        }

        public void UpdateButtonBoosterUsed(BoosterType type, BoosterStatus boosterStatus, Func<bool> checkIsInJail) {
            Input.BoosterButtonUsed(type, boosterStatus, checkIsInJail);
        }

        public void UpdateButtonBoosterFailToUse() {
            Input.OnFailedToUseBooster();
        }

        public void UpdateButtonInJail(BoosterStatus boosterStatus) {
            Input.OnPlayerInJail(boosterStatus);
        }

        public void UpdateButtonEndInJail(BoosterStatus boosterStatus) {
            Input.OnPlayerEndInJail(boosterStatus);
        }

        public void UpdateDamageUi(int value) {
            numItems[5].text = $"{value}";
        }

        public void UpdateFireUi(int value) {
            numItems[(int) ItemType.FireUp].text = $"{value}";
        }

        public void UpdateBombNumUi(int value) {
            numItems[(int) ItemType.BombUp].text = $"{value}";
        }

        public void UpdateSpeedUi(int value) {
            numItems[(int) ItemType.Boots].text = $"{value}";
        }

        public void UpdateItemUi(ItemType item, int value) {
            // fix lỗi sound không mong muốn khi load level
            if (value > 0) {
                _soundManager.PlaySound(Audio.GetItem);
            }
            numItems[(int) item].text = $"{value}";
        }

        public void SetQuantityBomb(int value) {
            Input.SetQuantityBomb(value);
        }

        public Text GetUiTextItem(ItemType item) {
            return numItems[(int) item];
        }

        public void ShowDialogOk(Canvas canvasDialog, string message) {
            DialogOK.ShowError(canvasDialog, message);
        }

        public void HideAllDialog(Canvas canvasDialog) {
            foreach (Transform child in canvasDialog.transform) {
                child.gameObject.SetActive(false);
            }
        }

        public void ShowDialogPvpVictory(
            Canvas canvasDialog,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            Action callback,
            bool isTournament,
            int[] boosters = null
        ) {
            DialogPvpVictory.Create().ContinueWith(dialogVictory => {
                dialogVictory.OnDidHide(() => { //
                    ShowDialogWin(canvasDialog, info, slot, rewardId, isOutOfChest, callback, isTournament, boosters);
                });
                dialogVictory.Show(canvasDialog);
            });
        }

        public void ShowDialogPvpDefeat(
            Canvas canvasDialog,
            LevelResult levelResult,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            Action callback,
            bool isTournament,
            int[] boosters = null
        ) {
            DialogPvpDefeat.Create().ContinueWith(dialogDefeat => {
                dialogDefeat.OnDidHide(() => {
                    ShowDialogPvpDrawOrLose(
                        canvasDialog, levelResult, info, slot, rewardId, isOutOfChest, callback, isTournament, boosters);
                });
                dialogDefeat.Show(canvasDialog);
            });
        }

        public void ShowDialogWin(
            Canvas canvasDialog,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            Action callback,
            bool isTournament,
            int[] boosters = null
        ) {
            _soundManager.PlaySound(Audio.PopupWin);
            BLDialogPvpWin.Create().ContinueWith(dialogWin => {
                if (isTournament) {
                    dialogWin.SetTournamentResult(slot, info, callback);
                } else {
                    dialogWin.SetRewards(info.Info[slot], rewardId, isOutOfChest, callback);
                }
                if (boosters != null) {
                    dialogWin.UpdateBooster(boosters);
                }
                dialogWin.OnDidShow(() => dialogWin._isShowDone = true);
                dialogWin.Show(canvasDialog);
            });
        }

        public void ShowDialogPvpDrawOrLose(
            Canvas canvasDialog,
            LevelResult levelResult,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            Action callback,
            bool isTournament,
            int[] boosters = null) {
            _soundManager.PlaySound(Audio.PopupDefeated);
            BLDialogPvpLose.Create().ContinueWith(dialogLose => {
                if (isTournament) {
                    dialogLose.SetTournamentResult(levelResult, info, slot, callback);
                } else {
                    dialogLose.SetRewards(levelResult, info.Info[slot], rewardId, isOutOfChest, callback);
                }
                if (boosters != null) {
                    dialogLose.UpdateBooster(boosters);
                }
                dialogLose.Show(canvasDialog);
            });
        }

        public void SetLatency(int slot, int latency) {
            SetLatencyText(latencyText[slot], latency);
        }

        public void SetMainLatency(int latency) {
            SetLatencyText(mainLatencyText, latency);
        }

        public void UpdateRemainTime(long remainTime) {
            if (remainTime < 0) {
                remainTime = 0;
            }
            var ts = TimeSpan.FromMilliseconds(remainTime);
            timerText.text = $"{ts.Minutes}:{ts.Seconds:D2}";
        }

        public void ShowHurryUp() {
            _soundManager.PlaySound(Audio.HurryUp);
            _hurryUp.Show();
        }

        public void InitHealthUi(int slot, int value) {
            UpdateHealthUi(slot, value);
        }

        public void ResetBoosterButton(BoosterStatus boosterStatus) {
            Input.ResetButtons(boosterStatus);
        }

        public void UpdateHealthUi(int slot, int value) {
            if (slot >= healthText.Length) {
                // FIXME: UI not supported.
                return;
            }
            var hp = value > 0 ? value : 0;
            healthText[slot].text = $"x{hp}";
        }

        public void FlyItemReward(Transform parent, Vector2 position, HeroItem item) {
            if (!flyingReward) {
                return;
            }

            var rewardObject = Instantiate(flyingReward, parent);
            var sprite = rewardObject.GetSprite(item);
            if (sprite == null) {
                Destroy(rewardObject.gameObject);
                return;
            }

            rewardObject.ChangeImage(sprite);
            var startPosition = _camera.WorldToScreenPoint(position);
            var endPosition = iconChest.transform.position;
            rewardObject.transform.position = startPosition;
            var move = rewardObject.transform.DOMove(endPosition, 1.0f);
            DOTween.Sequence()
                .Append(move)
                .OnComplete(() => { Destroy(rewardObject.gameObject); }
                );
        }

        private void SetLatencyText(Text text, int lag) {
            text.text = $"{lag}ms";
            text.color = lag switch {
                <= 100 => Color.Lerp(latencyColors[0], latencyColors[1], lag / 100f),
                <= 200 => Color.Lerp(latencyColors[1], latencyColors[2], (lag - 100) / 100f),
                <= 300 => Color.Lerp(latencyColors[2], latencyColors[3], (lag - 200) / 100f),
                _ => latencyColors[3]
            };
        }

        public void SetEnableInputPlantBomb(bool isEnable) {
            Input.SetEnableInputPlantBomb(isEnable);
        }

        public void SetEnableInputShield(bool isEnable) {
            Input.SetEnableInputShield(isEnable);
        }

        public void SetEnableInputKey(bool isEnable) {
            Input.SetEnableInputKey(isEnable);
        }

        public void SetVisible(ElementGui element, bool value) {
            switch (element) {
                case ElementGui.Joystick:
                case ElementGui.ButtonBomb:
                case ElementGui.ButtonKey:
                case ElementGui.ButtonShield:
                case ElementGui.BtBack:
                    Input.SetVisible(element, value);
                    break;
                case ElementGui.StatDamage:
                    stats[0].SetActive(value);
                    break;
                case ElementGui.StatBombUp:
                    stats[1].SetActive(value);
                    break;
                case ElementGui.StatFireUp:
                    stats[2].SetActive(value);
                    break;
                case ElementGui.StatBoots:
                    stats[3].SetActive(value);
                    break;
                case ElementGui.Timer:
                    timer.SetActive(value);
                    break;
                case ElementGui.HeroGroup:
                    heroGroup.SetActive(value);
                    break;
                case ElementGui.MatchId:
                    matchId.SetActive(value);
                    break;
                case ElementGui.ButtonEmoji:
                    Input.SetVisible(element, value);
                    break;
            }
        }

        #region PveGui

        public void UpdatePveInfo(BoosterStatus boosterStatus, bool displayPlayingTime, string stageLevel, bool isTesting) {
            throw new NotImplementedException();
        }

        public void AddEnemy(EnemyType enemyType) {
            throw new NotImplementedException();
        }

        public void RemoveEnemy(EnemyType enemyType) {
            throw new NotImplementedException();
        }

        public void UpdateSleepBossTime(int value) {
            throw new NotImplementedException();
        }

        public void ShowDialogPveWin(Canvas canvasDialog, int stage, int level, string rewardId, IWinReward[] rewards,
            Action callback) {
            throw new NotImplementedException();
        }

        public void ShowDialogPveLose(Canvas canvasDialog, int stage, int level, HeroTakeDamageInfo takeDamageInfo,
            StoryLoseCallback callback) {
            throw new NotImplementedException();
        }

        public void ShowDialogRate(Canvas canvasDialog, Action callback) {
            throw new NotImplementedException();
        }

        #endregion

        public Transform ButtonBombTransform => Input.ButtonBombTransform;
        public Transform ButtonShieldTransform => Input.ButtonShieldTransform;
        public Transform ButtonKeyTransform => Input.ButtonKeyTransform;
    }
}