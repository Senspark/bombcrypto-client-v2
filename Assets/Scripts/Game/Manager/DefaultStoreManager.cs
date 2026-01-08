using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics;

using BLPvpMode.Data;

using Newtonsoft.Json;

using CodeStage.AntiCheat.ObscuredTypes;

using PvpMode.UI;

using Senspark;

using UnityEngine;

namespace App {
    [Serializable]
    public enum ObserverCurrencyType {
        WalletBCoin, WalletSenBsc, Rock, WalletBomb, WalletSenPolygon
    }
    
    public class StoreManagerObserver {
        public Action OnVipRewardChanged;
        public Action<bool> OnAutoMineChanged;
        public Action OnRefreshAutoMine;
        public Action<string> OnNickNameChanged;
        public Action OnJoystickChoice;
    }
    
    public class PowerData
    {
        public int rare;
        public int[] power;
    }
    
    public enum SessionKey {
        BannerBirthday,
        CanBuyHeroesTrial,
    }
    
    public class BurnHeroData {
        public HeroId[] LastListHeroIdBurn;
        public string LastTx;
    }

    public class DefaultStoreManager : ObserverManager<StoreManagerObserver>, IStorageManager {
        public class Data {
            public string Username;
            public string Password;
            public string NickName;
            public bool IsJoyStick = false;
            public int SelectedHeroKey = -1;
            public bool EnableAutoWork;
            public bool CanBuyAutoMine;
            public DateTime LastOpened;
        }

        private class SessionData {
            private readonly bool[] _data = new bool[Enum.GetNames(typeof(SessionKey)).Length];
            private readonly Dictionary<string, object> _dict = new();
            public void SetSessionValue(SessionKey key, bool value) {
                _data[(int) key] = value;
            }

            public void SetSessionValue(string key, object value) {
                _dict[key] = value;
            }

            public bool GetSessionValue(SessionKey key) {
                return _data[(int) key];
            }

            public T GetSessionValue<T>(string key) {
                return (T) _dict[key];
            }

            public bool HasKey(string key) {
                return _dict.ContainsKey(key);
            }
        }

        private SessionData _sessionData;
        
        public int HeroLimit { get; set; }
        public int HeroTotalSale { get; set; }
        public int HouseLimit { get; set; }
        public BHeroPrice HeroPrice { get; set; }
        public double[,] UpgradePrice { get; set; } = new double[6, 5];
        public AbilityDesign[] HeroRandomizeAbilityCost { get; set; } = new AbilityDesign[6];
        public PowerData[] UpgradePower { get; set; } = new PowerData[5];
        public double[] HousePrice { get; set; } = new double[6];
        public int[] HouseMinAvailable { get; set; } = new int[6];
        public int[] HouseMintLimits { get; set; } = new int[6];
        public float[] Charge { get; set; } = new float[6];
        public int[] Slot { get; set; } = new int[6];
        public double[] FusionFee { get; set; } = new double[10];
        public double[] HousePriceTokenNetwork { get; set; } = new double[6];
        public double EndTimeTokenNetwork { get; set; }
        public IInvestedDetail InvestedDetail { get; set; }
        public List<IAutoMinePackageDetail> AutoMinePackages { get; set; }
        public IMinStakeHeroManager MinStakeHero { get; set; }
        public IRepairShieldConfig RepairShieldConfig { get; set; }
        public IRockPackConfigs RockPackConfigs { get; set; }
        public BurnHeroData LastBurnHeroData { get; set; }
        public IBurnHeroConfig BurnHeroConfig { get; set; }
        public IUpgradeShieldConfig UpgradeShieldConfig { get; set; }
        public IRentHousePackageConfigs RentHousePackConfigs { get; set; }

        public string Username {
            get => _data.Username;
            set {
                _data.Username = value;
                Save();
            }
        }

        public string IdTelegram { get; set; }

        public string Password {
            get => _data.Password;
            set {
                _data.Password = value;
                Save();
            }
        }

        public string MiningTokenType { get; set; }
        public bool IsEventSuperLegend { get; set; }

        public bool IsJoyStickChoice {
            get => _data.IsJoyStick;
            set {
                _data.IsJoyStick = value;
                Save();
                DispatchEvent(e => e.OnJoystickChoice?.Invoke());
            }
        }

        public int SelectedHeroKey {
            get => _data.SelectedHeroKey;
            set {
                _data.SelectedHeroKey = value;
                Save();
            }
        }
        
        public IVipStakeResponse VipStakeResults {
            get => _vipStakeResponse;
            set {
                _vipStakeResponse = value;
                DispatchEvent(e => e.OnVipRewardChanged?.Invoke());
            }
        }
        
        //public IRockPackage RockPackage { get; set; }
        
        public string NickName {
            get => _data.NickName;
            set {
                _data.NickName = value;
                Save();
                DispatchEvent(e => e.OnNickNameChanged?.Invoke(value));
            }
        }
        
        public TimeSpan StoryHunterRemainingTime { get; set; }
        public bool StoryHunterSeasonValid { get; set; }

        public bool EnableAutoMine {
            get => _data.EnableAutoWork;
            set {
                if (_data.EnableAutoWork == value) {
                    return;
                }
                _data.EnableAutoWork = value;
                DispatchEvent(e => e.OnAutoMineChanged?.Invoke(value));
                Save();
            }
        }

        public bool CanBuyAutoMine {
            get => _data.CanBuyAutoMine;
            set {
                _data.CanBuyAutoMine = value;
                DispatchEvent(e => e.OnRefreshAutoMine?.Invoke());
                Save();
            }
        }

        public string PvpMatchId { get; set; }
        public IAutoMineInfo AutoMineInfo { get; set; }
        public bool EnableBossHunter { get; set; }
  
        private ObscuredDouble _walletCoins;
        private ObscuredDouble _walletSens;
        private ObscuredInt _rockAmount;

        private IVipStakeResponse _vipStakeResponse;
        private readonly IDataManager _dataManager;
        private IDictionary<int, int> _pvpEquipments;
        private Data _data;

        public DefaultStoreManager(IDataManager dataManager) {
            _dataManager = dataManager;
        }

        public Task<bool> Initialize() {
            Load();
            _sessionData = new SessionData();
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private void Load() {
            const string nullContent = "{}";
            var content = _dataManager.GetString("store_manager", nullContent);
            try {
                _data = JsonConvert.DeserializeObject<Data>(content, new ObscuredValueConverter());
            } catch (Exception e) {
                Debug.Log(e.Message);
                _data = new Data();
                Save();
            }
        }

        public void LoadUpgradePowerFromServer(IHeroPower[] powerData) {
            UpgradePower = new PowerData[powerData.Length];
            for (var i = 0; i < powerData.Length; i++) {
                UpgradePower[i] = new PowerData {
                    rare = powerData[i].Rarity,
                    power = powerData[i].Powers,
                };
            }
        }

        public PowerData GetPowerData(int rare) {
            for (var i = 0; i < UpgradePower.Length; i++) {
                var power = UpgradePower[i];
                if (power != null && power.rare == rare) {
                    return power;
                }
            }
            return null;
        }

        public DateTime LastOpened {
            get => _data.LastOpened;
            set => _data.LastOpened = value;
        }

        public void UpdatePvPEquipment(IDictionary<int, int> equipments) {
            _pvpEquipments = equipments;
        }

        public void UpdatePvPEquipment(int equipmentId, int equipmentType) {
            _pvpEquipments[equipmentType] = equipmentId;
        }

        public void UpdateOpenChestRequiredShard(int quantity) {
            OpenChestRequiredSharp = quantity;
        }

        public void UpdateEventExp(long exp) {
            EventExp = exp;
        }

        public int OpenChestRequiredSharp { get; private set; }
        public long EventExp { get; private set; }
        public BoosterStatus PvPBoosters { get; set; }
        public bool NewUser { get; set; }
        
        public double[] RpcTokens { get; set; } = new double[Enum.GetNames(typeof(RpcTokenCategory)).Length];

        public int GetPvPEquipment(int equipmentType) {
            return _pvpEquipments.TryGetValue(equipmentType, out var equipmentId) ? equipmentId : -1;
        }

        public void SetSessionData(SessionKey key, bool value) {
            _sessionData.SetSessionValue(key, value);
        }

        public void SetSessionData(string key, object value) {
            _sessionData.SetSessionValue(key, value);
        }

        public bool GetSessionData(SessionKey key) {
            return _sessionData.GetSessionValue(key);
        }

        public T GetSessionData<T>(string key, object defaultValue) {
            if (_sessionData.HasKey(key)) {
                return _sessionData.GetSessionValue<T>(key);
            }
            return (T) defaultValue;
        }

        private void Save() {
            var content = JsonConvert.SerializeObject(_data, Formatting.None, new ObscuredValueConverter());
            _dataManager.SetString("store_manager", content);
        }
    }
}