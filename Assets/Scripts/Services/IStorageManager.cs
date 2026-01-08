using System;
using System.Collections.Generic;

using Analytics;

using BLPvpMode.Data;

using CodeStage.AntiCheat.ObscuredTypes;

using PvpMode.UI;

using Senspark;

namespace App {
    [Service(nameof(IStorageManager))]
    public interface IStorageManager : IService, IObserverManager<StoreManagerObserver> {
        int HeroLimit { get; set; }
        int HeroTotalSale { get; set; }
        int HouseLimit { get; set; }
        BHeroPrice HeroPrice { get; set; }
        double[,] UpgradePrice { get; set; }
        AbilityDesign[] HeroRandomizeAbilityCost { get; set; }
        PowerData[] UpgradePower { get; set; }
        double[] HousePrice { get; set; }
        int[] HouseMinAvailable { get; set; }
        int[] HouseMintLimits { get; set; }
        float[] Charge { get; set; }
        int[] Slot { get; set; }
        double[] FusionFee { get; set; }
        double[] HousePriceTokenNetwork { get; set; }
        double EndTimeTokenNetwork { get; set; }
        IInvestedDetail InvestedDetail { get; set; }
        string Username { get; set; }
        string IdTelegram { get; set; }
        string Password { get; set; }
        string MiningTokenType { get; set; }
        bool IsEventSuperLegend { get; set; }
        bool IsJoyStickChoice { get; set; }
        int SelectedHeroKey { get; set; }
        IVipStakeResponse VipStakeResults { get; set; }
        //IRockPackage RockPackage { get; set; }
        string NickName { get; set; }
        bool EnableAutoMine { get; set; }
        bool CanBuyAutoMine { get; set; }
        string PvpMatchId { set; get; }
        List<IAutoMinePackageDetail> AutoMinePackages { get; set; }
        IAutoMineInfo AutoMineInfo { get; set; }
        DateTime LastOpened { get; set; }
        int OpenChestRequiredSharp { get; }
        long EventExp { get; }
        BoosterStatus PvPBoosters { get; set; }
        bool NewUser { get; set; }
        IMinStakeHeroManager MinStakeHero { get; set; }
        IRepairShieldConfig RepairShieldConfig { get; set; }
        IRockPackConfigs RockPackConfigs { get; set; }
        BurnHeroData LastBurnHeroData { get; set; }
        IBurnHeroConfig BurnHeroConfig { get; set; }
        IUpgradeShieldConfig UpgradeShieldConfig { get; set; }//
        IRentHousePackageConfigs RentHousePackConfigs { get; set; }

        int GetPvPEquipment(int equipmentType);
        void LoadUpgradePowerFromServer(IHeroPower[] powerData);
        PowerData GetPowerData(int rare);
        void UpdatePvPEquipment(IDictionary<int, int> equipments);
        void UpdatePvPEquipment(int equipmentId, int equipmentType);
        void UpdateOpenChestRequiredShard(int quantity);
        void UpdateEventExp(long exp);
        void SetSessionData(SessionKey key, bool value);
        void SetSessionData(string key, object value);
        bool GetSessionData(SessionKey key);
        T GetSessionData<T>(string key, object defaultValue);//
    }
}
