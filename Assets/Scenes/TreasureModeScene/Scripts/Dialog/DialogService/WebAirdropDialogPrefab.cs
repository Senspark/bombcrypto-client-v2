using System;
using System.Collections.Generic;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using Scenes.TreasureModeScene.Scripts.Dialog;
using Share.Scripts.Dialog;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class WebAirdropDialogPrefab : DialogPrefab
{
    public AssetReference dialogOkAirdrop;
    public AssetReference dialogSettingAirdrop;
    public AssetReference bLDialogRewardAirdrop;
    public AssetReference dialogAutoMineAirdrop;
    public AssetReference dialogAutoMinePackageAirdrop;
    public AssetReference dialogFusionAirdrop;
    public AssetReference dialogFusionSkin;
    public AssetReference dialogInventoryAirdropForFusion;
    public AssetReference dialogInventoryAirdrop;
    public AssetReference dialogHouseAirdrop;
    public AssetReference dialogShopAirdrop;
    public AssetReference dialogHeroesAirdrop;
    public AssetReference dialogShopHouseWebAirdrop;
    public AssetReference dialogActiveHouseAirdrop;
    public AssetReference dialogHouseListAirdrop;
    public AssetReference dialogShopHeroWebAirdrop;
    public AssetReference dialogNewHeroAirdrop;
    public AssetReference dialogNewHouseAirdrop;
    public AssetReference dialogWarningBeforeBuyHouseAirdrop;
    public AssetReference dialogDepositAirdrop;
    public AssetReference dialogLeaderboardAirdrop;
    public AssetReference dialogWinAirdrop;
    public AssetReference dialogInformationAirdrop;
    public AssetReference dialogConfirmAirdrop;
    public AssetReference dialogMaxCapacityAirdrop;
    public AssetReference dialogInfoHouseAirdrop;
    public AssetReference dialogRentLandAirdrop;
    public AssetReference dialogNotEnoughRewardAirdrop;
    
    public override Dictionary<Type, AssetReference> GetAllDialogUsed() {
        return new Dictionary<Type, AssetReference> {
                { typeof(DialogOK), dialogOkAirdrop },
                { typeof(DialogSettingAirdrop), dialogSettingAirdrop },
                { typeof(BLDialogReward), bLDialogRewardAirdrop },
                { typeof(DialogAutoMine), dialogAutoMineAirdrop },
                { typeof(DialogAutoMinePackage), dialogAutoMinePackageAirdrop },
                { typeof(DialogFusionAirdrop), dialogFusionAirdrop },
                { typeof(DialogFusionSkin), dialogFusionSkin },
                { typeof(DialogInventoryAirdropForFusion), dialogInventoryAirdropForFusion },
                { typeof(DialogInventory), dialogInventoryAirdrop },
                { typeof(DialogHouse), dialogHouseAirdrop },
                { typeof(DialogShop), dialogShopAirdrop },
                { typeof(DialogHeroes), dialogHeroesAirdrop },
                { typeof(DialogShopHouseWebAirdrop), dialogShopHouseWebAirdrop },
                { typeof(DialogActiveHouse), dialogActiveHouseAirdrop },
                { typeof(DialogHouseListAirdrop), dialogHouseListAirdrop },
                { typeof(DialogShopHeroWebAirdrop), dialogShopHeroWebAirdrop },
                { typeof(DialogNewHero), dialogNewHeroAirdrop },
                { typeof(DialogNewHouseAirdrop), dialogNewHouseAirdrop },
                { typeof(DialogWarningBeforeBuyHouse), dialogWarningBeforeBuyHouseAirdrop },
                { typeof(DialogDepositAirdrop), dialogDepositAirdrop },
                { typeof(DialogLeaderboardAirdrop), dialogLeaderboardAirdrop },
                { typeof(DialogWin), dialogWinAirdrop },
                { typeof(DialogInformation), dialogInformationAirdrop },
                { typeof(DialogConfirm), dialogConfirmAirdrop },
                { typeof(DialogMaxCapacity), dialogMaxCapacityAirdrop },
                { typeof(DialogInfoHouse), dialogInfoHouseAirdrop },
                { typeof(DialogRentLandAirdrop), dialogRentLandAirdrop },
                { typeof(DialogNotEnoughRewardAirdrop), dialogNotEnoughRewardAirdrop },
            };
    }
}