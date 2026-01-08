using System.Collections.Generic;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using Scenes.PvpModeScene.Scripts;
using Scenes.TutorialScene.Scripts;

using UnityEngine;

namespace BLPvpMode.Test {
    public class TestShowDialog : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;
        
        [SerializeField]
        private BLGuiPvp guiPvp;

        [SerializeField]
        private BLLevelScenePvpSimulator levelScene;

        [Button]
        public void DialogPvpWin() {
            var pvpResultUserInfo = new PvpResultUserInfo("", false, 0, 0,
                "test", 0, 0, 0, 0, 0,
                new Dictionary<int, int>(), false,
                new Dictionary<int, float>());
            var userInfo = new IPvpResultUserInfo[] { pvpResultUserInfo };
            var pvpResultInfo = new PvpResultInfo("", 1, false, 0, new[] { 2, 1 }, userInfo);

            guiPvp.ShowDialogPvpVictory(canvasDialog, pvpResultInfo, 0, "", false, null, false);
        }

        [Button]
        public void DialogPvpLose() {
            var pvpResultUserInfo = new PvpResultUserInfo("", false, 0, 0,
                "test", 0, 0, 0, 0, 0,
                new Dictionary<int, int>(), false,
                new Dictionary<int, float>());
            var userInfo = new IPvpResultUserInfo[] { pvpResultUserInfo };
            var pvpResultInfo = new PvpResultInfo("", 1, false, 0, new[] { 2, 1 }, userInfo);
            guiPvp.ShowDialogPvpDefeat(canvasDialog, LevelResult.Lose, pvpResultInfo, 0, "", false, null, false);
        }

        [Button]
        public void DialogPvpDraw() {
            var pvpResultUserInfo = new PvpResultUserInfo("", false, 0, 0,
                "test", 0, 0, 0, 0, 0,
                new Dictionary<int, int>(), false,
                new Dictionary<int, float>());
            var userInfo = new IPvpResultUserInfo[] { pvpResultUserInfo };
            var pvpResultInfo = new PvpResultInfo("", 1, false, 0, new[] { 1, 2 }, userInfo);
            guiPvp.ShowDialogPvpDrawOrLose(canvasDialog, LevelResult.Draw, pvpResultInfo, 0, "", false, null, false);
        }

        [Button]
        public void ShowHurryUp() {
            guiPvp.ShowHurryUp();
        }
        
        [Button]
        public void FlyGoldReward(HeroItem item) {
            levelScene.SetItemToPlayer(0, 1, item);
        }
        
        [Button]
        public void PlayKillEffect(int slot) {
            levelScene.PlayKillEffectOnOther(slot);
        }
    }
}