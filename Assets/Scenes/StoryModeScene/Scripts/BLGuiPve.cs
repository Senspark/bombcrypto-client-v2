using System;
using System.Threading.Tasks;

using App;

using BLPvpMode;
using BLPvpMode.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.UI;

using BomberLand.Component;
using BomberLand.InGame;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Entities;
using Engine.Manager;

using Game.Dialog;
using Game.Dialog.BomberLand;
using Game.UI;

using PvpMode.Manager;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.StoryModeScene.Scripts {
    public class BLGuiPve : MonoObserverManager<BLGuiObserver>, IBLGui, IBLParticipantGui {
        [SerializeField]
        private Text playingTimeLbl;

        [SerializeField]
        private Text[] numItems;

        [SerializeField]
        private Text stageLevelText;

        [SerializeField]
        private BLHealthUI healthUI;

        [SerializeField]
        private EnemyAvatarControl enemyAvatarControl;

        [SerializeField]
        private Text countDownText;

        // Flying gold reward
        [SerializeField]
        private Transform iconChest;

        [SerializeField]
        private BLFlyingReward flyingReward;
        
        [SerializeField]
        private GameObject iconChestObj;

        private Camera _camera;
        private ISoundManager _soundManager;
        private BoosterStatus _boosterStatus;
        private IBLInputKey _input;

        public void Initialized(IBLInputKey input) {
            _input = input;
        }

        private void Awake() {
            _camera = Camera.main;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public IBLParticipantGui GetParticipantGui() {
            return this;
        }

        public void CheckInputKeyDown() {
            _input.CheckInputKeyDown();
        }

        public Vector2 GetDirectionFromInput() {
            return _input.GetDirection();
        }

        public void UpdatePveInfo(BoosterStatus boosterStatus, bool displayPlayingTime, string stageLevel, bool isTesting) {
            playingTimeLbl.gameObject.SetActive(displayPlayingTime);
            stageLevelText.text = stageLevel;
            _boosterStatus = boosterStatus;
            if (isTesting) {
                stageLevelText.gameObject.SetActive(false);
                iconChestObj.SetActive(false);
                var inputCallback = new PlayerInputCallback {
                    OnBombButtonClicked = OnBombButtonClicked,
                    OnBoosterButtonClicked = null,
                    OnBackButtonClicked = OnBackButtonClicked
                };
                _input.SetupButtons(_boosterStatus, inputCallback);
                _input.SetVisible(ElementGui.ButtonEmoji, false);
                _input.SetVisible(ElementGui.ButtonKey, false);
                _input.SetVisible(ElementGui.ButtonShield, false);
            } else {
                stageLevelText.gameObject.SetActive(true);
                iconChestObj.SetActive(true);
                var inputCallback = new PlayerInputCallback {
                    OnBombButtonClicked = OnBombButtonClicked,
                    OnBoosterButtonClicked = OnBoosterButtonClicked,
                    OnBackButtonClicked = OnBackButtonClicked
                };
                _input.SetupButtons(_boosterStatus, inputCallback);
                _input.ButtonKeyTransform.gameObject.SetActive(false);
                _input.SetVisible(ElementGui.ButtonEmoji, false);
            }
        }

        public void InitHealthUi(int slot, int value) {
            healthUI.Initialized(value);
        }

        public void ResetBoosterButton(BoosterStatus boosterStatus) {
            _input.ResetButtons(boosterStatus);
        }

        public void UpdateHealthUi(int slot, int value) {
            healthUI.UpdateHealth(value);
        }

        public void UpdateDamageUi(int value) {
            numItems[5].text = $"x{value}";
        }

        public void UpdateItemUi(ItemType item, int value) {
            numItems[(int) item].text = $"x{value}";
        }

        public void SetQuantityBomb(int value) {
            _input.SetQuantityBomb(value);
        }

        public void UpdateButtonInJail(BoosterStatus boosterStatus) {
            _input.OnPlayerInJail(boosterStatus);
        }

        public void UpdateButtonEndInJail(BoosterStatus boosterStatus) {
            _input.OnPlayerEndInJail(boosterStatus);
        }

        public void ShowDialogQuit(Canvas canvasDialog, Action onQuickCallback, Action onHideCallback) {
            _soundManager.PlaySound(Audio.Tap);
            DialogStoryQuit.Create().ContinueWith(dialog => {
                dialog.OnQuitCallback = onQuickCallback;
                dialog.OnDidHide(onHideCallback.Invoke);
                dialog.Show(canvasDialog);
            });
        }

        public void ShowDialogOk(Canvas canvasDialog, string message) {
            DialogOK.ShowError(canvasDialog, message);
        }

        public void UpdateButtonBoosterUsed(BoosterType type, BoosterStatus boosterStatus, Func<bool> checkIsInJail) {
            _input.BoosterButtonUsed(type, boosterStatus, checkIsInJail);
        }

        public void UpdateButtonBoosterFailToUse() {
            _input.OnFailedToUseBooster();
        }

        public void ShowErrorAndKick(Canvas canvasDialog, string reason) {
            Debug.LogError($"devv đáng lẽ phải kick: {reason}");
        }

        public async Task ShowDialogError(Canvas canvasDialog, string message) {
            var dialog = await DialogError.ShowErrorDialog(canvasDialog, message);
            await dialog.WaitForHide();
        }

        public void HideAllDialog(Canvas canvasDialog) {
            foreach (Transform child in canvasDialog.transform) {
                child.gameObject.SetActive(false);
            }
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

        public void UpdateSleepBossTime(int value) {
            if (value <= 0) {
                countDownText.text = "";
                return;
            }
            countDownText.text = $"{value}";
            countDownText.color = Color.Lerp(Color.red, Color.yellow, value / 60f);
        }

        #region MonoBehaviour

        private void OnBombButtonClicked() {
            DispatchEvent(e => e.SpawnBomb());
        }

        private void OnBoosterButtonClicked(BoosterType type) {
            DispatchEvent(e => e.UseBooster(type));
        }

        public void OnBackButtonClicked() {
            DispatchEvent(e => e.RequestQuit());
        }

        public void SetEnableInputPlantBomb(bool isEnable) {
            _input.SetEnableInputPlantBomb(isEnable);
        }

        #endregion

        #region PveGui

        public void AddEnemy(EnemyType enemyType) {
            enemyAvatarControl.AddEnemy(enemyType);
        }

        public void RemoveEnemy(EnemyType enemyType) {
            enemyAvatarControl.RemoveEnemy(enemyType);
        }

        public void ShowDialogPveWin(Canvas canvasDialog, int stage, int level,
            string rewardId, IWinReward[] rewards,
            Action callback
        ) {
            DialogStoryWin.Create().ContinueWith(dialog => {
                dialog.SetReward(stage, level, rewardId, rewards, callback);
                dialog.Show(canvasDialog);
            });
        }

        public void ShowDialogPveLose(Canvas canvasDialog, int stage, int level,
            HeroTakeDamageInfo takeDamageInfo,
            StoryLoseCallback callback) {
            DialogStoryLose.Create().ContinueWith(dialog => {
                dialog.SetInfo(stage, level, takeDamageInfo, callback);
                dialog.Show(canvasDialog);
            });
        }

        public void ShowDialogRate(Canvas canvasDialog, Action callback) {
            DialogRating.Create().ContinueWith(rating => {
                rating.OnCompleted(callback);
                rating.Show(canvasDialog);
            });
        }

        #endregion

        #region PvpGui

        public void InitEmojiButtons(int[] itemIds) {
            throw new NotImplementedException();
        }
        
        public void InitEmojiButtons() {
            throw new NotImplementedException();
        }
        
        public void ShowEmojiUi(int slot, int itemId) {
            throw new NotImplementedException();
        }

        public void UpdatePvpInfo(
            string matchId,
            string serverId,
            int slot,
            IMatchUserInfo[] pvpUsers,
            BoosterStatus boosterStatus) {
            throw new NotImplementedException();
        }

        public void SetMainLatency(int latency) {
            throw new NotImplementedException();
        }

        public void SetLatency(int slot, int latency) {
            throw new NotImplementedException();
        }

        public void UpdateRemainTime(long remainTime) {
            throw new NotImplementedException();
        }

        public void ShowDialogPvpVictory(
            Canvas canvasDialog,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            Action callback,
            bool isTournament,
            int[] boosters
        ) {
            throw new NotImplementedException();
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
            int[] boosters
        ) {
            throw new NotImplementedException();
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
            int[] boosters
        ) {
            throw new NotImplementedException();
        }

        public void ShowDialogPvpDraw(
            Canvas canvasDialog,
            Action exitCallback
        ) {
            throw new NotImplementedException();
        }

        public void ShowHurryUp() {
            throw new NotImplementedException();
        }

        public void SetVisible(ElementGui element, bool value) {
            throw new NotImplementedException();
        }

        #endregion
    }
}