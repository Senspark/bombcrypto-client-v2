using System;
using System.Collections.Generic;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using Scenes.TreasureModeScene.Scripts.Dialog;
using Share.Scripts.Dialog;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class TonDialogPrefab : DialogPrefab
{
    public AssetReference dialogOkTon;
    public AssetReference dialogSettingAirdrop;
    public AssetReference dialogReferral;
    public AssetReference dialogTask;
    public AssetReference dialogFusionAirdrop;
    public AssetReference dialogInventory;
    public AssetReference dialogHouseTon;
    public AssetReference dialogShop;
    public AssetReference dialogHeroesTon;
    public AssetReference dialogShopHouseAirdrop;
    public AssetReference dialogActiveHouseTon;
    public AssetReference dialogReferralTask;
    public AssetReference dialogHouseListAirdrop;
    public AssetReference dialogShopHeroTon;
    public AssetReference dialogNewHeroTon;
    public AssetReference dialogNewHouseAirdrop;
    public AssetReference dialogWarningBeforeBuyHouseTon;
    public AssetReference dialogDepositTon;
    public AssetReference bLDialogRewardTon;
    public AssetReference dialogLeaderboardAirdrop;
    public AssetReference dialogAutoMineTon;
    public AssetReference dialogAutoMinePackageTon;
    public AssetReference dialogWinTon;
    public AssetReference dialogOfflineRewardAirdrop;
    public AssetReference dialogInformationTon;
    public AssetReference dialogInventoryAirdropForFusion;
    public AssetReference dialogConfirmTon;
    public AssetReference dialogNotEnoughReward;
    public AssetReference dialogClubTon;
    public AssetReference dialogLockedTon;
    public AssetReference dialogConfirmReactiveHouse;
    public AssetReference dialogMaxCapacity;
    public AssetReference dialogFusionSkinTon;
    public AssetReference dialogRentLandAirdrop;
    public AssetReference dialogInfoHouseTon;
    
    public override Dictionary<Type, AssetReference> GetAllDialogUsed() {
        return new Dictionary<Type, AssetReference> {
                { typeof(DialogOK), dialogOkTon },
                { typeof(DialogSettingAirdrop), dialogSettingAirdrop },
                { typeof(DialogReferral), dialogReferral },
                { typeof(DialogTask), dialogTask },
                { typeof(DialogFusionAirdrop), dialogFusionAirdrop },
                { typeof(DialogInventory), dialogInventory },
                { typeof(DialogHouse), dialogHouseTon },
                { typeof(DialogShop), dialogShop },
                { typeof(DialogHeroes), dialogHeroesTon },
                { typeof(DialogShopHouseAirdrop), dialogShopHouseAirdrop },
                { typeof(DialogActiveHouse), dialogActiveHouseTon },
                { typeof(DialogReferralTask), dialogReferralTask },
                { typeof(DialogHouseListAirdrop), dialogHouseListAirdrop },
                { typeof(DialogShopHeroTon), dialogShopHeroTon },
                { typeof(DialogNewHero), dialogNewHeroTon },
                { typeof(DialogNewHouseAirdrop), dialogNewHouseAirdrop },
                { typeof(DialogWarningBeforeBuyHouse), dialogWarningBeforeBuyHouseTon },
                { typeof(DialogDepositAirdrop), dialogDepositTon },
                { typeof(BLDialogReward), bLDialogRewardTon },
                { typeof(DialogLeaderboardAirdrop), dialogLeaderboardAirdrop },
                { typeof(DialogAutoMine), dialogAutoMineTon },
                { typeof(DialogAutoMinePackage), dialogAutoMinePackageTon },
                { typeof(DialogWin), dialogWinTon },
                { typeof(DialogOfflineRewardAirdrop), dialogOfflineRewardAirdrop },
                { typeof(DialogInformation), dialogInformationTon },
                { typeof(DialogInventoryAirdropForFusion), dialogInventoryAirdropForFusion },
                { typeof(DialogConfirm), dialogConfirmTon },
                { typeof(DialogNotEnoughRewardAirdrop), dialogNotEnoughReward },
                { typeof(DialogClubAirdrop), dialogClubTon },
                { typeof(DialogLockedTon), dialogLockedTon },
                { typeof(DialogConfirmReactiveHouse), dialogConfirmReactiveHouse },
                { typeof(DialogMaxCapacity), dialogMaxCapacity },
                { typeof(DialogFusionSkin), dialogFusionSkinTon },
                { typeof(DialogRentLandAirdrop), dialogRentLandAirdrop },
                { typeof(DialogInfoHouse), dialogInfoHouseTon }
        };
    }
}
