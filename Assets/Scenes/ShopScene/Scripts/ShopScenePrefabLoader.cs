using System;
using System.Collections.Generic;

using Scenes.MainMenuScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.ShopScene.Scripts {
    public class ShopScenePrefabLoader : TemplatePrefabLoader {

        [SerializeField]
        private AssetReference dialogChestInfo;

        [SerializeField]
        private AssetReference dialogGachaChestInfo;

        [SerializeField]
        private AssetReference dialogGachaChestInfoPad;
        
        [SerializeField]
        private AssetReference dialogLuckyWheelReward;
        
        [SerializeField]
        private AssetReference dialogGachaChest;
        
        [SerializeField]
        private AssetReference dialogGachaMultiReward;

        [SerializeField]
        private AssetReference dialogSwapGemSuccess;

        [SerializeField]
        private AssetReference dialogAvatarReward;

        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogChestInfo), dialogChestInfo },
                { typeof(DialogGachaChestInfo), dialogGachaChestInfo },
                { typeof(DialogGachaChestInfoPad), dialogGachaChestInfoPad },
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(BLDialogGachaChest), dialogGachaChest},
                { typeof(BLDialogGachaMultiReward), dialogGachaMultiReward},
                { typeof(DialogSwapGemSuccess), dialogSwapGemSuccess},
                { typeof(BLDialogAvatarReward), dialogAvatarReward},
            };
            base.Initialize();
        }

    }
}
