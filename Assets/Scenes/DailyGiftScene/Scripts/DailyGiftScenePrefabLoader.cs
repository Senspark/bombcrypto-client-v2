using System;
using System.Collections.Generic;

using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.DailyGiftScene.Scripts {
    public class DailyGiftScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogLuckyWheelReward;
        [SerializeField]
        private AssetReference dialogRewardTutorial;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X3;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X6;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(DialogRewardTutorial), dialogRewardTutorial },
                { typeof(DialogLuckyWheelReward2X3), dialogLuckyWheelReward2X3 },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
            };
            base.Initialize();
        }
    }
}