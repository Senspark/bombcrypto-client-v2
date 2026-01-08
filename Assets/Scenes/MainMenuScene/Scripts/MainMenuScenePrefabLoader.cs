using System;
using System.Collections.Generic;

using Game.Dialog;

using Scenes.ConnectScene.Scripts;
using Scenes.HeroesScene.Scripts;
using Scenes.PvpModeScene.Scripts;
using Scenes.ShopScene.Scripts;
using Scenes.StoryModeScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Scenes.MainMenuScene.Scripts {
    public class MainMenuScenePrefabLoader : TemplatePrefabLoader {
        public AssetReference dialogSetting;
        public AssetReference dialogControlSetting;
        public AssetReference dialogLeaderboard;
        public AssetReference dialogLeaderboardPad;
        public AssetReference dialogLeaderboardInformation;
        public AssetReference dialogPointDecay;
        public AssetReference dialogProfile;
        public AssetReference dialogLuckyWheelReward2X6;
        public AssetReference dialogPvPReward;
        public AssetReference dialogPvpRankDown;
        public AssetReference dialogPvpRankUp;
        public AssetReference dialogPvpSchedule;
        public AssetReference dialogPvpStream;
        public AssetReference afDialogRename;
        public AssetReference dialogGachaChest;
        public AssetReference dialogGachaMultiReward;
        public AssetReference dialogHeroSelection;
        public AssetReference dialogDeleteAccount;
        public AssetReference dialogIntroduction;
        public AssetReference dialogIapPack;
        public AssetReference dialogEquipment;
        public AssetReference dialogChooseNetworkServer;
        public AssetReference dialogDailyTask;
        public AssetReference dialogGachaChestInfo;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference> {
                { typeof(DialogSetting), dialogSetting },
                { typeof(DialogControlSetting), dialogControlSetting },
                { typeof(DialogLeaderboard), dialogLeaderboard },
                { typeof(DialogLeaderboardPad), dialogLeaderboardPad },
                { typeof(DialogLeaderboardInformation), dialogLeaderboardInformation },
                { typeof(DialogPointDecay), dialogPointDecay },
                { typeof(DialogProfile), dialogProfile },
                { typeof(DialogLuckyWheelReward2X6), dialogLuckyWheelReward2X6 },
                { typeof(BLDialogPvPReward), dialogPvPReward },
                { typeof(DialogPvpRankDown), dialogPvpRankDown },
                { typeof(DialogPvpRankUp), dialogPvpRankUp },
                { typeof(DialogPvpSchedule), dialogPvpSchedule },
                { typeof(DialogPvpStream), dialogPvpStream },
                { typeof(AfDialogRename), afDialogRename},
                { typeof(BLDialogGachaChest), dialogGachaChest},
                { typeof(BLDialogGachaMultiReward), dialogGachaMultiReward},
                { typeof(DialogHeroSelection), dialogHeroSelection},
                { typeof(DialogDeleteAccount), dialogDeleteAccount },
                { typeof(DialogIntroduction), dialogIntroduction },
                { typeof(BLDialogIapPack), dialogIapPack},
                { typeof(DialogEquipment), dialogEquipment},
                { typeof(DialogChooseNetworkServer), dialogChooseNetworkServer},
                { typeof(DialogDailyTask), dialogDailyTask},
                { typeof(DialogGachaChestInfo), dialogGachaChestInfo },
            };
            base.Initialize();
        }
    }
}
