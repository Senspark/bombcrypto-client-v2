using System;
using System.Collections.Generic;

using Scenes.MainMenuScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.PvpModeScene.Scripts {
    public class PvpModeScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogPvpQuit;
        [SerializeField]
        private AssetReference dialogPvpVictory;
        [SerializeField]
        private AssetReference dialogPvpWin;
        [SerializeField]
        private AssetReference dialogPvpDefeat;
        [SerializeField]
        private AssetReference dialogPvpLose;
        [SerializeField]
        private AssetReference dialogPvpDraw;
        
        [SerializeField]
        private AssetReference dialogBonusReward;
        [SerializeField]
        private AssetReference dialogLuckyWheel;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward;
        [SerializeField]
        private AssetReference dialogRewardTutorial;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X3;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X6;
        [SerializeField]
        private AssetReference dialogGachaChest;
        [SerializeField]
        private AssetReference dialogGachaMultiReward;

        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogPvpQuit), dialogPvpQuit },
                { typeof(DialogPvpVictory), dialogPvpVictory },
                { typeof(BLDialogPvpWin), dialogPvpWin },
                { typeof(DialogPvpDefeat), dialogPvpDefeat },
                { typeof(BLDialogPvpLose), dialogPvpLose },
                { typeof(BLDialogPvpDraw), dialogPvpDraw },
                { typeof(DialogBonusReward), dialogBonusReward },
                { typeof(BLDialogLuckyWheel), dialogLuckyWheel },
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(DialogRewardTutorial), dialogRewardTutorial },
                { typeof(DialogLuckyWheelReward2X3), dialogLuckyWheelReward2X3 },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
                { typeof(BLDialogGachaChest), dialogGachaChest},
                { typeof(BLDialogGachaMultiReward), dialogGachaMultiReward}
            };
            base.Initialize();
        }
    }
}