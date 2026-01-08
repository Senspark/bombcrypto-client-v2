using System;
using System.Collections.Generic;

using Scenes.ShopScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.AltarScene.Scripts {
    public class AltarScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogChestInfo;

        [SerializeField]
        private AssetReference dialogFuse;

        [SerializeField]
        private AssetReference dialogGrind;
        
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
                { typeof(DialogChestInfo), dialogChestInfo },
                { typeof(DialogFuse), dialogFuse },
                { typeof(DialogGrind), dialogGrind },
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(DialogRewardTutorial), dialogRewardTutorial },
                { typeof(DialogLuckyWheelReward2X3), dialogLuckyWheelReward2X3 },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
            };
            base.Initialize();
        }
    }
}