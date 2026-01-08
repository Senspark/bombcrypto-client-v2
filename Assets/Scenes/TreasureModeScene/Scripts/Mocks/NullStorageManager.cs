using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using BLPvpMode.Data;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullStorageManager : IStorageManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public int AddObserver(StoreManagerObserver observer) {
            return 0;
        }

        public bool RemoveObserver(int id) {
            return true;
        }

        public void DispatchEvent(Action<StoreManagerObserver> dispatcher) {
            throw new NotImplementedException();
        }

        public int HeroLimit { get; set; }
        public int HeroTotalSale { get; set; }
        public int HouseLimit { get; set; }
        public BHeroPrice HeroPrice { get; set; }
        public double[,] UpgradePrice { get; set; }
        public AbilityDesign[] HeroRandomizeAbilityCost { get; set; }
        public PowerData[] UpgradePower { get; set; }
        public double[] HousePrice { get; set; }
        public int[] HouseMinAvailable { get; set; }
        public int[] HouseMintLimits { get; set; }
        public float[] Charge { get; set; }
        public int[] Slot { get; set; }
        public double[] FusionFee { get; set; }
        public double[] HousePriceTokenNetwork { get; set; }
        public double EndTimeTokenNetwork { get; set; }
        public IInvestedDetail InvestedDetail { get; set; }
        public string Username { get; set; }
        public string IdTelegram { get; set; }
        public string Password { get; set; }
        public string MiningTokenType { get; set; }
        public bool IsEventSuperLegend { get; set; }
        public bool IsJoyStickChoice { get; set; }
        public int SelectedHeroKey { get; set; }
        public IVipStakeResponse VipStakeResults { get; set; }
        public string NickName { get; set; }
        public bool EnableAutoMine { get; set; }
        public bool CanBuyAutoMine { get; set; }
        public string PvpMatchId { get; set; }
        public List<IAutoMinePackageDetail> AutoMinePackages { get; set; }
        public IAutoMineInfo AutoMineInfo { get; set; }
        public DateTime LastOpened { get; set; }
        public int OpenChestRequiredSharp { get; }
        public long EventExp { get; }
        public BoosterStatus PvPBoosters { get; set; }
        public bool NewUser { get; set; }
        public bool PassCodeEntered { get; set; }
        public string PassCode { get; set; }
        public IMinStakeHeroManager MinStakeHero { get; set; }
        public IRepairShieldConfig RepairShieldConfig { get; set; }
        public IRockPackConfigs RockPackConfigs { get; set; }
        public BurnHeroData LastBurnHeroData { get; set; }
        public IBurnHeroConfig BurnHeroConfig { get; set; }
        public IUpgradeShieldConfig UpgradeShieldConfig { get; set; }
        public IRentHousePackageConfigs RentHousePackConfigs { get; set; }
        public int GetPvPEquipment(int equipmentType) {
            throw new NotImplementedException();
        }

        public void LoadUpgradePowerFromServer(IHeroPower[] powerData) {
            throw new NotImplementedException();
        }

        public PowerData GetPowerData(int rare) {
            throw new NotImplementedException();
        }

        public void UpdatePvPEquipment(IDictionary<int, int> equipments) {
            throw new NotImplementedException();
        }

        public void UpdatePvPEquipment(int equipmentId, int equipmentType) {
            throw new NotImplementedException();
        }

        public void UpdateOpenChestRequiredShard(int quantity) {
            throw new NotImplementedException();
        }

        public void UpdateEventExp(long exp) {
            throw new NotImplementedException();
        }

        public void SetSessionData(SessionKey key, bool value) {
            throw new NotImplementedException();
        }

        public void SetSessionData(string key, object value) {
            throw new NotImplementedException();
        }

        public bool GetSessionData(SessionKey key) {
            throw new NotImplementedException();
        }

        public T GetSessionData<T>(string key, object defaultValue) {
            throw new NotImplementedException();
        }
    }
}