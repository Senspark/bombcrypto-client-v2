using System;
using System.Collections.Generic;

using Game.Dialog;

using Scenes.MainMenuScene.Scripts;

using Share.Scripts.PrefabsManager;

using UnityEngine.AddressableAssets;

namespace Scenes.FarmingScene.Scripts {
    public class FarmingScenePrefabLoader : TemplatePrefabLoader {
        public AssetReference dialogWin;
        public AssetReference dialogSetting;
        public AssetReference dialogFusionPolygon;
        public AssetReference dialogInventory;
        public AssetReference dialogInventoryPad;
        public AssetReference dialogHouse;
        public AssetReference dialogHouseHelp;
        public AssetReference dialogHouseList;
        public AssetReference dialogHouseListPad;
        public AssetReference dialogShop;
        public AssetReference dialogShopCoin;
        public AssetReference dialogShopHero;
        public AssetReference dialogShopHouse;
        public AssetReference dialogShopHousePad;
        public AssetReference dialogHeroes;
        public AssetReference dialogHeroesPad;
        public AssetReference dialogActiveHouse;
        public AssetReference dialogNewHero;
        public AssetReference dialogNewHouse;
        public AssetReference dialogWarningBeforeBuyHouse;
        public AssetReference dialogDeposit;
        public AssetReference bLDialogReward;
        public AssetReference dialogLegacyHeroes;
        public AssetReference dialogConfirmSelect;
        public AssetReference dialogAutoMine;
        public AssetReference dialogAutoMinePackage;
        public AssetReference dialogBCoinReward;
        public AssetReference dialogSelectStaking;
        public AssetReference dialogStakeHeroesS;
        public AssetReference dialogStakeHeroesPlus;
        public AssetReference dialogUnStakingConfirm;
        public AssetReference dialogUnStakingResult;
        public AssetReference dialogStaking;
        public AssetReference dialogConfirmStake;
        public AssetReference dialogConfirmUnStake;
        public AssetReference dialogStakingResult;
        public AssetReference dialogSmithyPolygon;
        public AssetReference dialogSmithyPad;
        public AssetReference dialogConfirmBuyRock;
        public AssetReference dialogForge;
        public AssetReference dialogConfirmBurnOrFusion;
        public AssetReference dialogCommunityLink;
        
        public override void Initialize() {
            Map = new Dictionary<Type, AssetReference>() {
                {typeof(DialogWin), dialogWin },
                {typeof(DialogSetting), dialogSetting},
                {typeof(DialogFusionPolygon), dialogFusionPolygon},
                {typeof(DialogInventory), dialogInventory},
                {typeof(DialogInventoryPad), dialogInventoryPad},
                {typeof(DialogHouse), dialogHouse},
                {typeof(DialogHouseHelp), dialogHouseHelp},
                {typeof(DialogHouseList), dialogHouseList},
                {typeof(DialogHouseListPad), dialogHouseListPad},
                {typeof(DialogShop), dialogShop},
                {typeof(DialogShopCoin), dialogShopCoin},
                {typeof(DialogShopHero), dialogShopHero},
                {typeof(DialogShopHouse), dialogShopHouse},
                {typeof(DialogShopHousePad), dialogShopHousePad},
                {typeof(DialogHeroes), dialogHeroes},
                {typeof(DialogHeroesPad), dialogHeroesPad},
                {typeof(DialogActiveHouse), dialogActiveHouse},
                {typeof(DialogNewHero), dialogNewHero},
                {typeof(DialogNewHouse), dialogNewHouse},
                {typeof(DialogWarningBeforeBuyHouse), dialogWarningBeforeBuyHouse},
                {typeof(DialogDeposit), dialogDeposit},
                {typeof(BLDialogReward), bLDialogReward},
                {typeof(DialogLegacyHeroes), dialogLegacyHeroes},
                {typeof(DialogConfirmSelect), dialogConfirmSelect},
                {typeof(DialogAutoMine), dialogAutoMine},
                {typeof(DialogAutoMinePackage), dialogAutoMinePackage},
                {typeof(DialogBCoinReward), dialogBCoinReward},
                {typeof(DialogSelectStaking), dialogSelectStaking},
                {typeof(DialogStakeHeroesS), dialogStakeHeroesS},
                {typeof(DialogStakeHeroesPlus), dialogStakeHeroesPlus},
                {typeof(DialogUnStakingConfirm), dialogUnStakingConfirm},
                {typeof(DialogUnStakingResult), dialogUnStakingResult},
                {typeof(DialogStaking), dialogStaking},
                {typeof(DialogConfirmStake), dialogConfirmStake},
                {typeof(DialogConfirmUnStake), dialogConfirmUnStake},
                {typeof(DialogStakingResult), dialogStakingResult},
                {typeof(DialogSmithyPolygon), dialogSmithyPolygon},
                {typeof(DialogSmithyPad), dialogSmithyPad},
                {typeof(DialogConfirmBuyRock), dialogConfirmBuyRock},
                {typeof(DialogForge), dialogForge},
                {typeof(DialogConfirmBurnOrFusion), dialogConfirmBurnOrFusion},
                {typeof(DialogCommunityLink), dialogCommunityLink},
            };
            base.Initialize();
        }
    }
}