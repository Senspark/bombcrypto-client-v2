using System;
using System.Collections.Generic;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using Scenes.TreasureModeScene.Scripts.Dialog;
using Share.Scripts.Dialog;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class SolanaDialogPrefab : DialogPrefab
{
    public AssetReference dialogOkSolana;
    public AssetReference dialogSettingSolana;
    public AssetReference dialogInventorySolana;
    public AssetReference dialogHouseSolana;
    public AssetReference dialogShopSolana;
    public AssetReference dialogHeroesSolana;
    public AssetReference dialogShopHouseSolana;
    public AssetReference dialogActiveHouseSolana;
    public AssetReference dialogReferralTask;
    public AssetReference dialogHouseListSolana;
    public AssetReference dialogShopHeroSolana;
    public AssetReference dialogNewHeroSolana;
    public AssetReference dialogNewHouseSolana;
    public AssetReference dialogWarningBeforeBuyHouseSolana;
    public AssetReference dialogDepositAirdrop;
    public AssetReference bLDialogRewardSolana;
    public AssetReference dialogLeaderboardSolana;
    public AssetReference dialogAutoMineSolana;
    public AssetReference dialogAutoMinePackageSolana;
    public AssetReference dialogWinSolana;
    public AssetReference dialogOfflineRewardAirdrop;
    public AssetReference dialogInformationSolana;
    public AssetReference dialogConfirmSolana;
    public AssetReference dialogFusionSolana;
    public AssetReference dialogMaxCapacitySolana;
    public AssetReference dialogFusionSkinSolana;
    public AssetReference dialogInventorySolanaForFusion;
    public AssetReference dialogInfoHouseSolana;
    public AssetReference dialogRentLandSolana;
    public AssetReference dialogNotEnoughRewardAirdrop;
    public AssetReference dialogLockedSolana;
    public AssetReference dialogConfirmReactiveHouseSolana;
    
    public override Dictionary<Type, AssetReference> GetAllDialogUsed() {
        return new Dictionary<Type, AssetReference> {
                { typeof(DialogOK), dialogOkSolana },
                { typeof(DialogSettingAirdrop), dialogSettingSolana },
                { typeof(DialogFusionAirdrop), dialogFusionSolana },
                { typeof(DialogInventory), dialogInventorySolana },
                { typeof(DialogHouse), dialogHouseSolana },
                { typeof(DialogShop), dialogShopSolana },
                { typeof(DialogHeroes), dialogHeroesSolana },
                { typeof(DialogShopHouseAirdrop), dialogShopHouseSolana },
                { typeof(DialogActiveHouse), dialogActiveHouseSolana },
                { typeof(DialogReferralTask), dialogReferralTask },
                { typeof(DialogHouseListAirdrop), dialogHouseListSolana },
                { typeof(DialogShopHeroSolana), dialogShopHeroSolana },
                { typeof(DialogNewHero), dialogNewHeroSolana },
                { typeof(DialogNewHouseAirdrop), dialogNewHouseSolana },
                { typeof(DialogWarningBeforeBuyHouse), dialogWarningBeforeBuyHouseSolana },
                { typeof(DialogDepositAirdrop), dialogDepositAirdrop },
                { typeof(BLDialogReward), bLDialogRewardSolana },
                { typeof(DialogLeaderboardAirdrop), dialogLeaderboardSolana },
                { typeof(DialogAutoMine), dialogAutoMineSolana },
                { typeof(DialogAutoMinePackage), dialogAutoMinePackageSolana },
                { typeof(DialogWin), dialogWinSolana },
                { typeof(DialogOfflineRewardAirdrop), dialogOfflineRewardAirdrop },
                { typeof(DialogInformation), dialogInformationSolana },
                { typeof(DialogConfirm), dialogConfirmSolana },
                { typeof(DialogMaxCapacity), dialogMaxCapacitySolana },
                { typeof(DialogFusionSkin), dialogFusionSkinSolana },
                { typeof(DialogInventoryAirdropForFusion), dialogInventorySolanaForFusion },
                { typeof(DialogInfoHouse), dialogInfoHouseSolana },
                { typeof(DialogRentLandAirdrop), dialogRentLandSolana },
                { typeof(DialogNotEnoughRewardAirdrop), dialogNotEnoughRewardAirdrop },
                { typeof(DialogLockedTon), dialogLockedSolana },
                { typeof(DialogConfirmReactiveHouse), dialogConfirmReactiveHouseSolana },
            };
    }
}