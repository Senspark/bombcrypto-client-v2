using System;
using System.Collections.Generic;

using Game.Dialog.BomberLand;

using Scenes.MainMenuScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.StoryModeScene.Scripts {
    public class StoryModeScenePrefabLoader : TemplatePrefabLoader {
        
        [SerializeField]
        private AssetReference dialogStoryQuit;
        [SerializeField]
        private AssetReference dialogStoryWin;
        [SerializeField]
        private AssetReference dialogStoryLose;
        
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
        private AssetReference dialogRating;
        [SerializeField]
        private AssetReference dialogGachaChest;
        [SerializeField]
        private AssetReference dialogGachaMultiReward;
        [SerializeField]
        private AssetReference dialogFeedback;

        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogStoryQuit), dialogStoryQuit },
                { typeof(DialogStoryWin), dialogStoryWin },
                { typeof(DialogStoryLose), dialogStoryLose },
                { typeof(DialogBonusReward), dialogBonusReward },
                { typeof(BLDialogLuckyWheel), dialogLuckyWheel },
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(DialogRewardTutorial), dialogRewardTutorial },
                { typeof(DialogLuckyWheelReward2X3), dialogLuckyWheelReward2X3 },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
                { typeof(DialogRating), dialogRating},
                { typeof(BLDialogGachaChest), dialogGachaChest},
                { typeof(BLDialogGachaMultiReward), dialogGachaMultiReward},
                { typeof(DialogFeedback), dialogFeedback},
            };
            base.Initialize();
        }
    }
}