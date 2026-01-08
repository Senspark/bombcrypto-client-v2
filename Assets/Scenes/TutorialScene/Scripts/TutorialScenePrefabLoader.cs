using System;
using System.Collections.Generic;

using Scenes.ConnectScene.Scripts;
using Scenes.HeroesScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.TutorialScene.Scripts {
    public class TutorialScenePrefabLoader : TemplatePrefabLoader {
        [SerializeField]
        private AssetReference dialogMaintenance;
        [SerializeField]
        private AssetReference dialogEquipment;
        [SerializeField]
        private AssetReference dialogChooseNetworkServer;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward;
        [SerializeField]
        private AssetReference dialogRewardTutorial;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X3;
        [SerializeField]
        private AssetReference dialogLuckyWheelReward2X6;
        [SerializeField]
        private AssetReference dialogCheckConnection;
        [SerializeField]
        private AssetReference dialogRequestNewGuestAccount;

        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                { typeof(DialogMaintenance), dialogMaintenance },
                { typeof(DialogEquipment), dialogEquipment },
                { typeof(DialogChooseNetworkServer), dialogChooseNetworkServer },
                { typeof(BLDialogLuckyWheelReward), dialogLuckyWheelReward },
                { typeof(DialogRewardTutorial), dialogRewardTutorial },
                { typeof(DialogLuckyWheelReward2X3), dialogLuckyWheelReward2X3 },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
                { typeof(AfDialogCheckConnection), dialogCheckConnection },
                { typeof(DialogRequestNewGuestAccount), dialogRequestNewGuestAccount}
            };
            base.Initialize();
        }
    }
}