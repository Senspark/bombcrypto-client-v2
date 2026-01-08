using System.Threading.Tasks;
using App;
using BLPvpMode.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using Engine.Entities;
using Engine.Manager;
using Game.Dialog;
using Game.UI;
using JetBrains.Annotations;
using PvpMode.Manager;

using Scenes.PvpModeScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using UnityEngine;

namespace BLPvpMode.UI {
    public enum ElementGui {
        Joystick,
        ButtonBomb,
        ButtonKey,
        ButtonShield,
        StatDamage,
        StatBombUp,
        StatFireUp,
        StatBoots,
        Timer,
        HeroGroup,
        MatchId,
        BtBack,
        ButtonEmoji,
    }

    public interface IBLGui {
        IBLParticipantGui GetParticipantGui();
        void CheckInputKeyDown();

        void UpdatePvpInfo(
            [NotNull] string matchId,
            [CanBeNull] string serverId,
            int slot,
            [NotNull] IMatchUserInfo[] pvpUsers,
            [NotNull] BoosterStatus boosterStatus);

        void UpdatePveInfo(BoosterStatus boosterStatus, bool displayPlayingTime, string stageLevel, bool isTesting = false);
        void InitHealthUi(int slot, int value);
        void InitEmojiButtons(int[] itemIds);
        void ShowEmojiUi(int slot, int itemId);

        void UpdateRemainTime(long remainTime);
        void ResetBoosterButton(BoosterStatus boosterStatus);
        void UpdateHealthUi(int slot, int value);
        void UpdateDamageUi(int value);
        void UpdateItemUi(ItemType item, int value);
        void UpdateButtonInJail(BoosterStatus boosterStatus);
        void UpdateButtonEndInJail(BoosterStatus boosterStatus);
        void UpdateButtonBoosterUsed(BoosterType type, BoosterStatus boosterStatus, System.Func<bool> checkIsInJail);
        void UpdateButtonBoosterFailToUse();
        void SetMainLatency(int latency);
        void SetLatency(int slot, int latency);
        void SetQuantityBomb(int value);

        void FlyItemReward(Transform parent, Vector2 position, HeroItem item);

        void ShowDialogQuit(Canvas canvasDialog, System.Action onQuickCallback, System.Action onHideCallback);
        void ShowDialogOk(Canvas canvasDialog, string message);
        Task ShowDialogError(Canvas canvasDialog, string message);
        void ShowErrorAndKick(Canvas canvasDialog, string reason);
        void HideAllDialog(Canvas canvasDialog);

        #region PvpGui

        void ShowDialogPvpVictory(
            Canvas canvasDialog,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            System.Action callback,
            bool isTournament,
            int[] boosters = null
        );

        void ShowDialogPvpDefeat(
            Canvas canvasDialog,
            LevelResult levelResult,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            System.Action callback,
            bool isTournament,
            int[] boosters = null
        );

        void ShowDialogPvpDrawOrLose(
            Canvas canvasDialog,
            LevelResult levelResult,
            IPvpResultInfo info,
            int slot,
            string rewardId,
            bool isOutOfChest,
            System.Action callback,
            bool isTournament,
            int[] boosters = null
        );

        void ShowHurryUp();
        public void SetVisible(ElementGui element, bool value);

        #endregion

        #region PveGui

        void AddEnemy(EnemyType enemyType);
        void RemoveEnemy(EnemyType enemyType);
        void UpdateSleepBossTime(int value);

        void ShowDialogPveWin(Canvas canvasDialog, int stage, int level,
            string rewardId, IWinReward[] rewards,
            System.Action callback
        );

        void ShowDialogPveLose(Canvas canvasDialog, int stage, int level,
            HeroTakeDamageInfo takeDamageInfo,
            StoryLoseCallback callback);

        void ShowDialogRate(Canvas canvasDialog, System.Action callback);

        #endregion
    }
}