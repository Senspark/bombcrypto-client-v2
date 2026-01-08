using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CodeStage.AntiCheat.ObscuredTypes;
using Data;
using Engine.Entities;

using Newtonsoft.Json;
using Server.Models;
using Sfs2X.Entities.Data;
using Story.Strategy.Level;
using Story.Strategy.Provider;
using SuperTiled2Unity;
using UnityEngine;

namespace App.BomberLand {
    public partial class DefaultGeneralServerBridge {
        private class SendExtensionRequestResult<T> {
            [JsonProperty("ec")]
            public int Code;
        
            [JsonProperty("data")]
            public T Data;
        
            [JsonProperty("es")]
            public string Message;
        }
        
        private class HeroPower : IHeroPower {
            [JsonProperty("rare")]
            public int Rarity { get; set; }

            [JsonProperty("power")]
            public int[] Powers { get; set; }
        }

        private class HouseDetails : IHouseDetails {
            private readonly ObscuredString _details;
            public string Details => _details;

            public int Id { get; }
            public int Rarity { get; }
            public float Recovery { get; }
            public int Capacity { get; }
            public bool IsActive { get; }
            public long EndTimeRent { get; }

            public static IHouseDetails Parse(ISFSObject data) {
                var details = data.GetUtfString("house_gen_id");
                var active = data.GetInt("active") == 1;
                var endTimeRent = 0L;
                if (data.ContainsKey("end_time_rent")) {
                    endTimeRent = data.GetLong("end_time_rent");
                }
                return new HouseDetails(details, active, endTimeRent);
            }
            
            public static IHouseDetails[] ParseOldSeasonArray(ISFSObject data) {
                var detailsArray = data.GetSFSArray(SFSDefine.SFSField.OldSeason);
                var details = new IHouseDetails[detailsArray.Size()];
                for (var i = 0; i < detailsArray.Size(); ++i) {
                    var entry = detailsArray.GetSFSObject(i);
                    details[i] = Parse(entry);
                }
                return details;
            }
            

            private HouseDetails(string details, bool active, long endTimeRent) {
                _details = details;
                var detailsInt = BigInteger.Parse(details);
                Id = (int) (detailsInt & ((1 << 30) - 1));
                Rarity = (int) ((detailsInt >> 40) & 31);
                Recovery = (int) ((detailsInt >> 45) & ((1 << 15) - 1)) / 60f;
                Capacity = (int) ((detailsInt >> 60) & 31);
                IsActive = active;
                EndTimeRent = endTimeRent;
            }
        }

        private class AutoMinePackage : IAutoMinePackages {
            public IAutoMineInfo Info { get; }
            public List<IAutoMinePackageDetail> Packages { get; }

            public AutoMinePackage(ISFSObject data) {
                var endTimeKey = "last_package_end_time";
                var endTimeLong = !data.IsNull(endTimeKey) ? data.GetLong(endTimeKey) : 0;
                var endTime = DateTimeOffset.FromUnixTimeMilliseconds(endTimeLong).DateTime.ToLocalTime();
                Info = new AutoMineInfo(endTime);
                Packages = new List<IAutoMinePackageDetail>(); 
                var packages = data.GetSFSArray("packages");
                for (var i = 0; i < packages.Count; i++) {
                    var p = packages.GetSFSObject(i);
                    Packages.Add(new AutoMinePackageDetail(p));
                }
                Packages.Sort((a, b) => a.Price.CompareTo(b.Price));
            }
        }

        private class AutoMinePackageDetail : IAutoMinePackageDetail {
            public string Package { get; }
            public double Price { get; }

            public AutoMinePackageDetail(ISFSObject data) {
                Package = data.GetUtfString("package");
                Price = data.GetDouble("price");
            }
        }

        private class AutoMineInfo : IAutoMineInfo {
            public bool CanBuyAutoMine => EndTime.AddDays(-2) <= DateTime.Now;

            public bool ActiveAutoMine => DateTime.Now < EndTime;
            public DateTime EndTime { get; }
            
            public AutoMineInfo(ISFSObject data) {
                var endTime = DateTimeOffset.FromUnixTimeMilliseconds(data.GetLong("end_time")).DateTime.ToLocalTime();
                EndTime = endTime;
            }

            public AutoMineInfo(DateTime endTime) {
                EndTime = endTime;
            }
        }
        private class RockPackage : IRockPackage {
            public int RockReceived { get; }
            public int RockTotal { get; }

            public RockPackage(ISFSObject data) {
                RockReceived = data.GetInt("rock_received");
                RockTotal = data.GetInt("rock_total");
            }
        }

        private class VoucherResponse : IVoucherResponse {
            public bool Allow { get; }
            public string Amount { get; }
            public string Signature { get; }
            public int Nonce { get; }
            public int HeroQuantity { get; }

            public VoucherResponse(ISFSObject data) {
                Signature = data.GetUtfString("signature");
                Amount = data.GetUtfString("amount");
                Nonce = data.GetInt("nonce");
                HeroQuantity = data.GetInt("heroQuantity");
                Allow = !string.IsNullOrWhiteSpace(Signature);
            }
        }

        

        

        [Serializable]
        public class LevelConfig {
            public LevelStrategy[] levelStrategies;

            [JsonConstructor]
            public LevelConfig(LevelStrategy[] levels) {
                levelStrategies = levels;
            }
        }

        [Serializable]
        public class EnemiesConfig {
            public Dictionary<EnemyType, EnemyCreator> enemyCreators;

            [JsonConstructor]
            public EnemiesConfig(EnemyCreator[] entities) {
                enemyCreators = new Dictionary<EnemyType, EnemyCreator>();
                foreach (var iter in entities) {
                    enemyCreators[iter.Type] = iter;
                }
            }
        }

        private class SyncHeroResponse : ISyncHeroResponse {
            public IHeroDetails[] Details { get; }
            public int[] NewIds { get; }
            public int HeroesSize { get; }

            public SyncHeroResponse(ISFSObject data) {
                Details = HeroDetails.ParseArray(data);
                var newIdsArray = data.GetSFSArray(SFSDefine.SFSField.NewBombers);
                var newIdsJson = newIdsArray.ToJson();
                NewIds = JsonConvert.DeserializeObject<int[]>(newIdsJson);
                if (data.ContainsKey(SFSDefine.SFSField.HeroesSize)) {
                    HeroesSize = data.GetInt(SFSDefine.SFSField.HeroesSize);
                } else {
                    HeroesSize = Details.Length;
                }
            }

            public SyncHeroResponse(IHeroDetails[] details) {
                Details = details;
                NewIds = Array.Empty<int>();
            }
        }

        private class BuyTrialHeroResponse : ISyncHeroResponse {
            public IHeroDetails[] Details { get; }
            public int[] NewIds { get; }
            
            public BuyTrialHeroResponse(ISFSObject data) {
                Details = HeroDetails.ParseArray(data);
                NewIds = Details.Select(e => e.Id).ToArray();
            }
            
            public BuyTrialHeroResponse(IHeroDetails[] details, int[] newIds) {
                Details = details;
                NewIds = newIds;
            }
        }

        private class BuyHeroTonResponse : IBuyHeroServerResponse {
            public IHeroDetails[] Details { get; }

            public BuyHeroTonResponse(ISFSObject data) {
                Details = HeroDetails.ParseArray(data);
            }
        }
        
        private class SyncHouseResponse : ISyncHouseResponse {
            public IHouseDetails[] Details { get; }
            public int[] NewIds { get; }
            public IHouseDetails[] OldSeasonDetails { get; }
            public Dictionary<int, List<int>> HeroInHouse { get; }

            public SyncHouseResponse(ISFSObject data) {
                var detailsArray = data.GetSFSArray(SFSDefine.SFSField.Houses);
                Details = new IHouseDetails[detailsArray.Size()];
                for (var i = 0; i < detailsArray.Size(); ++i) {
                    var entry = detailsArray.GetSFSObject(i);
                    Details[i] = HouseDetails.Parse(entry);
                }
                var newIdsArray = data.GetSFSArray(SFSDefine.SFSField.NewHouses);
                var newIdsJson = newIdsArray.ToJson();
                NewIds = JsonConvert.DeserializeObject<int[]>(newIdsJson);
                if (data.ContainsKey(SFSDefine.SFSField.OldSeason)) {
                    OldSeasonDetails = HouseDetails.ParseOldSeasonArray(data);
                }
                HeroInHouse = new Dictionary<int, List<int>>();
                if (data.ContainsKey("hero_in_house")) {
                    var heroesInHouse = data.GetSFSArray("hero_in_house");
                    for (var i = 0; i < heroesInHouse.Size(); ++i) {
                        var entry = heroesInHouse.GetSFSObject(i);
                        HeroInHouse[entry.GetInt("house_id")] = entry.GetIntArray("list_heroes").ToList();
                    }
                }
            }
        }
        
        private class OnOfflineReward: IOfflineReward {
            public OnOfflineReward(ISFSObject data) {
                offlineTime = data.GetDouble("time_offline");
                rewardType = data.GetInt("reward_type");
                amount = data.GetDouble("quantity");
            }

            public double offlineTime { get; }
            public int rewardType { get; }
            public double amount { get; }
        }

        

        private class ResetNerfFee : IResetNerfFee {
            public float BcoinCost { get; }
            public float SenCost { get; }
            public int HeroQuantity { get; }

            public ResetNerfFee(float bcoinCost, float senCost, int heroQuantity) {
                BcoinCost = bcoinCost;
                SenCost = senCost;
                HeroQuantity = heroQuantity;
            }
        }

        private class VipStakeResponse : IVipStakeResponse {
            public List<IVipBooster> Inventory { get; }
            public List<IVipInfo> Rewards { get; }

            public VipStakeResponse(ISFSObject data) {
                var boostersArr = data.GetSFSArray("boosters");
                var rewardsArr = data.GetSFSArray("rewards");

                Inventory = new List<IVipBooster>();
                foreach (ISFSObject b in boostersArr) {
                    Inventory.Add(new VipBooster(b));
                }

                Rewards = new List<IVipInfo>();
                foreach (ISFSObject r in rewardsArr) {
                    Rewards.Add(new VipInfo(r));
                }
            }
        }

        private class VipInfo : IVipInfo {
            public double StakeAmount { get; }
            public List<IVipReward> Rewards { get; }
            public int VipLevel { get; }
            public bool IsCurrentVip { get; }

            public VipInfo(ISFSObject data) {
                StakeAmount = data.GetDouble("stake_amount");
                IsCurrentVip = data.GetBool("currentVip");
                VipLevel = data.GetInt("level");

                Rewards = new List<IVipReward>();
                var rewards = data.GetSFSArray("reward");
                foreach (ISFSObject r in rewards) {
                    Rewards.Add(new VipReward(r));
                }
            }
        }

        private class VipBooster : IVipBooster {
            public int Quantity { get; }
            public VipRewardType Type { get; }

            public VipBooster(ISFSObject data) {
                Quantity = data.GetInt("quantity");
                Type = data.GetUtfString("type") switch {
                    "SHIELD" => VipRewardType.Shield,
                    "CONQUEST_CARD" => VipRewardType.ConquestCard,
                    "BOMBERMAN" => VipRewardType.Hero,
                    "PROTECTION_CARD" => VipRewardType.ProtectionCard,
                    "PREMIUM_PROTECTION_CARD" => VipRewardType.PremiumProtectionCard,
                    "PREMIUM_CONQUEST_CARD" => VipRewardType.PremiumConquestCard,
                    "COMBO_DAILY" => VipRewardType.ComboDaily,
                    _ => throw new Exception("Invalid Data")
                };
            }
        }

        private class VipReward : VipBooster, IVipReward {
            public int HavingQuantity { get; }
            public DateTime NextClaimUtc { get; }

            public VipReward(ISFSObject data) : base(data) {
                HavingQuantity = data.GetInt("havingQuantity");
                NextClaimUtc = DateTimeOffset.FromUnixTimeMilliseconds(data.GetLong("nextClaim")).DateTime;

                // Not using yet
                // data.GetUtfString("rewardType"); // "BOOSTER" || "REWARD"
                // data.GetInt("dates"); // 1 || 15 
            }
        }

        private class StakeResult : IStakeResult {
            public double MyStake { get; set; }
            public double WithdrawFee { get; set; }
            public DateTime StakeDate { get; set; }
            public double DPY { get; set; }
            public double TotalStake { get; set; }
            public double ReceiveAmount { get; set; }
            public double Profit { get; set; }
        }

        private class AirDropEvent : IAirDropEvent {
            public DateTime OpenDate { get; }
            public DateTime CloseDate { get; }
            public TimeSpan RemainingTime { get; }
            public string CodeName { get; }
            public string EventName { get; }
            public int BomberToBuy { get; }
            public int BomberBought { get; }
            public int RewardAmount { get; }
            public AirDropClaimStatus ClaimStatus { get; }
            public bool Closed { get; }
            public int SupplyTotal { get; }
            public int SupplyClaimed { get; }

            public AirDropEvent(ISFSObject data) {
                var openDate = data.GetLong("openDate");
                var closeDate = data.GetLong("closeDate");
                OpenDate = DateTimeOffset.FromUnixTimeMilliseconds(openDate).DateTime;
                CloseDate = DateTimeOffset.FromUnixTimeMilliseconds(closeDate).DateTime;
                CodeName = data.GetUtfString("code");
                EventName = data.GetUtfString("name");
                BomberToBuy = data.GetInt("bomberToBuy");
                BomberBought = data.GetInt("bomberBought");
                RewardAmount = data.GetInt("rewardAmount");
                var claimStatus = data.GetUtfString("claimStatus");
                ClaimStatus = claimStatus switch {
                    "COMPLETED" => AirDropClaimStatus.Completed,
                    "RETRY" => AirDropClaimStatus.Retry,
                    "PENDING" => AirDropClaimStatus.Pending,
                    _ => AirDropClaimStatus.Checking,
                };
                SupplyTotal = data.GetInt("rewardTotal");
                SupplyClaimed = data.GetInt("rewardClaimed");

                var now = DateTime.UtcNow;
                Closed = CloseDate <= now || SupplyClaimed >= SupplyTotal;
                if (!Closed) {
                    RemainingTime = CloseDate - now;
                } else {
                    RemainingTime = TimeSpan.Zero;
                }
            }
        }

        private class AirDropResponse : IAirDropResponse {
            public List<IAirDropEvent> ActiveEvents { get; }
            public List<IAirDropEvent> ClosedEvents { get; }

            public AirDropResponse(ISFSArray array) {
                ActiveEvents = new List<IAirDropEvent>();
                ClosedEvents = new List<IAirDropEvent>();
                for (var i = 0; i < array.Size(); i++) {
                    var obj = new AirDropEvent(array.GetSFSObject(i));
                    if (obj.Closed) {
                        ClosedEvents.Add(obj);
                    } else {
                        ActiveEvents.Add(obj);
                    }
                }
            }
        }

        private class AirDropClaimResponse : IAirDropClaimResponse {
            public IAirDropResponse Events { get; }
            public int Amount { get; }
            public int EventId { get; }
            public int Nonce { get; }
            public string Signature { get; }

            public AirDropClaimResponse(ISFSObject data) {
                var airDrop = data.GetSFSArray("airdrops");
                Events = new AirDropResponse(airDrop);
                Amount = data.GetInt("numClaim");
                EventId = data.GetInt("eventID");
                Nonce = data.GetInt("nonce");
                Signature = data.GetUtfString("signature");
            }
        }

        private class LuckyReward : ILuckyReward {
            public int Quantity { get; }
            public string Type { get; }

            public LuckyReward(ISFSObject data) {
                Quantity = data.GetInt("reward_quantity");
                Type = data.GetUtfString("type");
            }
        }

        private class LuckyRewardsResponse : ILuckyRewardsResponse {
            public List<ILuckyReward> RewardsList { get; }
            
            public LuckyRewardsResponse(ISFSObject data) {
                var arr = data.GetSFSArray("data");
                RewardsList = new List<ILuckyReward>();
                for (var i = 0; i < arr.Size(); i++) {
                    RewardsList.Add(new LuckyReward(arr.GetSFSObject(i)));
                }
            }
        }
        
        private class DailyMission : IDailyMission {
            public string Mission { get; }
            public string MissionCode { get; }
            public bool Claimable { get; }
            public int TicketReward { get; }
            public int RequestTimes { get; }
            public int CompletedTimes { get; }

            public DailyMission(ISFSObject data) {
                Mission = data.GetUtfString("mission");
                MissionCode = data.GetUtfString("code");
                Claimable = data.GetBool("claimable");
                TicketReward = data.GetInt("ticketRewardNumb");
                RequestTimes = data.GetInt("times");
                CompletedTimes = data.GetInt("numberCompletedMission");
            }
        }

        private class DailyMissionListResponse : IDailyMissionListResponse {
            public List<IDailyMission> Missions { get; }
            
            public DailyMissionListResponse(ISFSObject data) {
                var arr = data.GetSFSArray("data");
                Missions = new List<IDailyMission>();
                for (var i = 0; i < arr.Size(); i++) {
                    Missions.Add(new DailyMission(arr.GetSFSObject(i)));
                }
            }
        }

        private class EmailResponse : IEmailResponse {
            public bool Verified { get; }
            public string Email { get; }

            public EmailResponse(ISFSObject data) {
                Verified = data.GetInt("verified") == 1;
                Email = data.GetUtfString("email");
            }
        }

        private class ServerConfigResponse : IServerConfigResponse {
            public struct Hero {
                [JsonProperty("bomb")]
                public int Bomb;

                [JsonProperty("bomb_range")]
                public int BombRange;

                [JsonProperty("speed")]
                public int Speed;

                [JsonProperty("color")]
                public int Color;

                [JsonProperty("item_id")]
                public int ItemId;

                [JsonProperty("maxBomb")]
                public int MaxBomb;

                [JsonProperty("maxRange")]
                public int MaxRange;

                [JsonProperty("maxSpeed")]
                public int MaxSpeed;

                [JsonProperty("skin")]
                public int Skin;

                [JsonProperty("tutorial")]
                public int Tutorial;
            }
            
            public bool CanBuyHeroesTrial { get; }
            public ProductData[] Products { get; }
            public Hero[] Heroes { get; }
            public bool IsGetTrHero { get; }
            public TimeSpan SkinItemExpiryTime { get; }

            public ServerConfigResponse(ISFSObject data) {
                CanBuyHeroesTrial = data.GetBool("is_buy_heroes_trial");
                Products = JsonConvert.DeserializeObject<ProductData[]>(data.GetSFSArray("product_items").ToJson());
                Heroes = JsonConvert.DeserializeObject<Hero[]>(data.GetSFSArray("config_hero_traditional").ToJson());
                IsGetTrHero = data.GetBool("is_get_hero_tr");
                SkinItemExpiryTime = TimeSpan.FromMilliseconds(data.GetLong("skin_item_expiry_time"));
            }
        }

        private class MinStakeHeroManager : IMinStakeHeroManager {
            // Min stake bcoin để nâng cấp hero từ L lên L+
            public Dictionary<HeroRarity, int> MinStakeLegacy { get; }

            // Min stake bcoin để nhận bcoin TH mode v1
            public Dictionary<HeroRarity, int> MinStakeGetBcoin { get; }

            // Min stake sen để nhận sen TH mode v1
            public Dictionary<HeroRarity, int> MinStakeGetSen { get; }

            public MinStakeHeroManager(ISFSObject data) {
                MinStakeLegacy = new Dictionary<HeroRarity, int>();
                MinStakeGetBcoin = new Dictionary<HeroRarity, int>();
                MinStakeGetSen = new Dictionary<HeroRarity, int>();
                var json = data.GetSFSArray("data");
                for (var i = 0; i < json.Count; i++) {
                    var j = json.GetSFSObject(i);
                    var heroRarity = (HeroRarity)j.GetInt("rarity");
                    MinStakeLegacy[heroRarity] = j.GetInt("min_stake_amount");
                    MinStakeGetBcoin[heroRarity] = j.GetInt("min_stake_bcoin");
                    MinStakeGetSen[heroRarity] = j.GetInt("min_stake_sen");
                }
            }
        }

        private class RepairShieldConfig : IRepairShieldConfig {
            public Dictionary<int, Dictionary<int, float>> Data { get; }
            public RepairShieldConfig(ISFSObject data) {
                Data = new Dictionary<int, Dictionary<int, float>>();
                var json = data.GetSFSArray("data");
                for (var i = 0; i < json.Size(); i++) {
                    var arr = json.GetSFSArray(i);
                    var rewardForRarity = new Dictionary<int, float>();
                    for (int j = 0; j < arr.Size(); j++) {
                        var obj = arr.GetSFSObject(j);
                        var shieldLevel = obj.GetInt("shield_level");
                        var priceRock = obj.GetFloat("price_rock");
                        rewardForRarity.Add(shieldLevel, priceRock);
                    }
                    Data.Add(i, rewardForRarity);
                }
                Debug.Log(Data);
            }
        }
        
        private class RockPackConfigs : IRockPackConfigs {
            public List<IRockPackConfig> Packages { get; set; }
            
            public RockPackConfigs(ISFSObject data) {
                var arr = data.GetSFSArray("data");
                Packages = new List<IRockPackConfig>();
                for (var i = 0; i < arr.Size(); i++) {
                    Packages.Add(new RockPackConfig(arr.GetSFSObject(i)));
                }
            }
        }
        
        private class RockPackConfig : IRockPackConfig {
            public string PackageName { get; set; }
            public int RockAmount { get; set; }
            public double SenPrice { get; set; }
            public double BcoinPrice { get; set; }
            
            public RockPackConfig(ISFSObject data) {
                PackageName = data.GetUtfString("pack_name");
                RockAmount = data.GetInt("rock_amount");
                SenPrice = data.GetDouble("sen_price");
                BcoinPrice = data.GetDouble("bcoin_price");
                
            }
        }

        private class RentHousePackageConfigs : IRentHousePackageConfigs {
            public List<IRentHousePackageConfig> Packages { get; set; }
            
            public RentHousePackageConfigs(ISFSObject data) {
                var arr = data.GetSFSArray("data");
                Packages = new List<IRentHousePackageConfig>();
                for (var i = 0; i < arr.Size(); i++) {
                    Packages.Add(new RentHousePackageConfig(arr.GetSFSObject(i)));
                }
            }
        }
        
        private class RentHousePackageConfig : IRentHousePackageConfig {
            public int Rarity { get; set; }
            public float Price { get; set; }
            public int NumDays { get; set; }
            
            public RentHousePackageConfig(ISFSObject data) {
                Rarity = data.GetInt("rarity");
                Price = data.GetFloat("price");
                NumDays = data.GetInt("num_days");
                
            }
        }

        private class TreasureHuntConfigResponse : ITreasureHuntConfigResponse {
            public int HeroLimit { get; }
            public BHeroPrice HeroPrice { get; }
            public double[,] HeroUpgradePrice { get; }
            public AbilityDesign[] HeroAbilityDesign { get; }
            public int HouseLimit { get; }
            public double[] HousePrices { get; }
            public int[] HouseMintLimits { get; }
            public HouseStats[] HouseStats { get; }
            public double[] FusionFee { get; }
            public double[] HousePriceTokenNetwork { get; }
            public double EndTimeTokenNetwork { get; }

            public TreasureHuntConfigResponse(ISFSObject data) {
                // hero limit
                HeroLimit = int.TryParse(data.GetUtfString("hero_limit"), out var heroLimit) ? heroLimit : 0; 
                
                // hero price
                try {
                    HeroPrice = JsonConvert.DeserializeObject<BHeroPrice>(data.GetUtfString("hero_price"));
                } catch {
                    // FIXME: mạng TON trả về mãng khác với các mạng khác là trả về chuỗi, cần xem lại để đồng bộ kiểu.
                    //DevHoang: Add new airdrop
                    var ton = 0f;
                    var starCore = 0f;
                    var sol = 0f;
                    var bcoinDeposited = 0f;
                    var ron = 0f;
                    var bas = 0f;
                    var vic = 0f;
                    var prices = JsonConvert.DeserializeObject<List<BHeroPrice>>(data.GetUtfString("hero_price"));
                    foreach (var price in prices) {
                        ton += price.Ton;
                        starCore += price.StarCore;
                        sol += price.Sol;
                        bcoinDeposited += price.BcoinDeposited;
                        ron += price.Ron;
                        bas += price.Bas;
                        vic += price.Vic;
                    }
                    HeroPrice = new BHeroPrice(0f, 0f, ton, starCore, bcoinDeposited, sol, ron, bas, vic);
                }

                // hero upgrade price
                var heroUpgradeCode = data.GetUtfString("hero_upgrade_cost");
                if (!string.IsNullOrEmpty(heroUpgradeCode)) {
                    HeroUpgradePrice = JsonConvert.DeserializeObject<double[,]>(heroUpgradeCode);
                }
                
    
                var heroAbilityDesign = data.GetUtfString("hero_ability_designs");
                HeroAbilityDesign = heroAbilityDesign == null? Array.Empty<AbilityDesign>():  JsonConvert.DeserializeObject<AbilityDesign[]>(data.GetUtfString("hero_ability_designs"));
                
                // house - limit, price, available, mint linits
                var houseLimit = data.GetUtfString("house_limit");
                HouseLimit = houseLimit == null ? 0: data.GetUtfString("house_limit").ToInt();
                var housePrices = data.GetUtfString("house_prices");
                HousePrices = JsonConvert.DeserializeObject<double[]>(housePrices);
                var houseMintLimits = data.GetUtfString("house_mint_limits");
                HouseMintLimits =  houseMintLimits == null ? new int[6]: JsonConvert.DeserializeObject<int[]>(houseMintLimits);
                
                // house stats
                var houseStats = data.GetUtfString("house_stats");
                HouseStats = JsonConvert.DeserializeObject<HouseStats[]>(houseStats);
                
                // fusion fee
                if (data.ContainsKey("fusion_fee")) {
                    var fusionFee = data.GetUtfString("fusion_fee");
                    FusionFee = JsonConvert.DeserializeObject<double[]>(fusionFee);
                }
                
                // TON - house prices
                if (data.ContainsKey("house_prices_token_network")) {
                    var housePricesTon = data.GetUtfString("house_prices_token_network");
                    HousePriceTokenNetwork = JsonConvert.DeserializeObject<double[]>(housePricesTon);
                }
                
                // TON: end time buy by ton
                if (data.ContainsKey("disable_buy_with_token_network")) {
                    var endTimeBuyTon = data.GetUtfString("disable_buy_with_token_network");
                    EndTimeTokenNetwork = JsonConvert.DeserializeObject<double>(endTimeBuyTon);
                }
            }
        }

        private class BurnHeroConfig : IBurnHeroConfig {
            public Dictionary<HeroRarity, IRockBurnData> Data { get; }
            public BurnHeroConfig(ISFSObject data) {
                Data = new Dictionary<HeroRarity, IRockBurnData>();
                var json = data.GetSFSArray("data");
                for (var i = 0; i < json.Count; i++) {
                    var j = json.GetSFSObject(i);
                    Data[(HeroRarity)j.GetInt("rarity")] = new RockBurnData(j);
                }
            }
        }

        private class RockBurnData : IRockBurnData {
            public float heroSRock { get; }
            public float heroLRock { get; }
            
            public RockBurnData(ISFSObject data) {
                heroSRock = data.GetFloat("heroS");
                heroLRock = data.GetFloat("heroL");
            }
        }
        
        private class UpgradeShieldConfig : IUpgradeShieldConfig {
            public Dictionary<HeroRarity, List<int>> DurabilityPoint { get; }
            public Dictionary<HeroRarity, List<float>> PriceRock { get; }
            public UpgradeShieldConfig(ISFSObject data) {
                DurabilityPoint = new Dictionary<HeroRarity, List<int>>();
                PriceRock = new Dictionary<HeroRarity, List<float>>();
                var json = data.GetSFSArray("data");
                for (var i = 0; i < json.Count; i++) {
                    var j = json.GetSFSObject(i);
                    var rarity = (HeroRarity)j.GetInt("rarity");
                    DurabilityPoint[rarity] = JsonConvert.DeserializeObject<List<int>>(j.GetUtfString("durability_point"));
                    PriceRock[rarity] = JsonConvert.DeserializeObject<List<float>>(j.GetUtfString("price_rock"));
                }
            }
        }
        
        private class UpgradeShieldResponse : IUpgradeShieldResponse {
            public int Nonce { get; }
            public string Signature { get; }
            
            public UpgradeShieldResponse(ISFSObject data) {
                Nonce = data.GetInt("nonce");
                Signature = data.GetUtfString("signature");
            }
        }
        
        public class ReferralData : IReferralData
        {
            public string referralCode { get; set; }
            public int minClaimReferral { get; set; }
            public int timePayOutReferral { get; set; }
            public int childQuantity { get; set; }
            public double rewards { get; set; }
            
            public ReferralData(ISFSObject data) {
                referralCode = data.GetUtfString("referral_code");
                minClaimReferral = data.GetInt("min_claim_referral");
                timePayOutReferral = data.GetInt("time_pay_out_referral");
                childQuantity = data.GetInt("child_quantity");
                rewards = data.GetDouble("rewards");
            }
        }
        
        private class FusionTonHeroResponse : IFusionTonHeroResponse {
            public bool Result { get; }
            public IHeroDetails[] Details { get; }
            public int[] NewIds { get; }
            public int HeroesSize { get; }

            public FusionTonHeroResponse(ISFSObject data) {
                Result = data.GetBool("result");
                Details = HeroDetails.ParseArray(data);
                var newIdsArray = data.GetSFSArray(SFSDefine.SFSField.NewBombers);
                var newIdsJson = newIdsArray.ToJson();
                NewIds = JsonConvert.DeserializeObject<int[]>(newIdsJson);
                if (data.ContainsKey(SFSDefine.SFSField.HeroesSize)) {
                    HeroesSize = data.GetInt(SFSDefine.SFSField.HeroesSize);
                } else {
                    HeroesSize = -1;
                }
            }
        }
        
        public class MemberClubInfo : IMemberClubInfo
        {
            public string Name { get; }
            public double PointTotal { get; }
            public double PointCurrentSeason { get; }
            
            public MemberClubInfo(ISFSObject data) {
                Name = data.GetUtfString("name");
                PointTotal = data.GetDouble("point_total");
                PointCurrentSeason = data.GetDouble("point_current_season");
            }
        }
        
        public class ClubInfo : IClubInfo
        {
            public string ReferralCode { get; }
            public long ClubId { get; }
            public string Name { get; }
            public string Link { get; }
            public double PointTotal { get; }
            public double PointCurrentSeason { get; }
            public IMemberClubInfo[] Members { get; set; }
            public IMemberClubInfo CurrentMember { get; set; }
            public bool IsTopBidClub { get; }
            public byte[] Avatar { get; }

            public ClubInfo(ISFSObject data) {
                if (data.ContainsKey("referral_code")) {
                    ReferralCode = data.GetUtfString("referral_code");
                }
                ClubId = data.GetLong("club_id");
                Name = data.GetUtfString("club_name");
                Link = data.GetUtfString("club_link");
                PointTotal = data.GetDouble("club_point_total");
                PointCurrentSeason = data.GetDouble("club_point_current_season");
                var membersArray = data.GetSFSArray("club_members");
                Members = new IMemberClubInfo[membersArray.Count];
                for (var i = 0; i < membersArray.Count; i++) {
                    Members[i] = new MemberClubInfo(membersArray.GetSFSObject(i));
                }
                if (data.ContainsKey("current_member")) {
                    CurrentMember = new MemberClubInfo(data.GetSFSObject("current_member"));
                }
                IsTopBidClub = data.GetBool("is_top_bid_club");
                if (data.ContainsKey("club_avatar")) {
                    var clubAvatarArray = data.GetSFSArray("club_avatar");
                    var byteList = new List<byte>();
                    for (var i = 0; i < clubAvatarArray.Size(); i++)
                    {
                        byteList.Add(clubAvatarArray.GetByte(i)); 
                    }
                    Avatar = byteList.ToArray();
                } else {
                    Avatar = null;
                }
            }
        }

        public class ClubRank : IClubRank {
            public long ClubId { get; }
            public string Name { get; }
            public double PointTotal { get; }
            public double PointCurrentSeason { get; }

            public ClubRank(ISFSObject data) {
                ClubId = data.GetLong("club_id");
                Name = data.GetUtfString("club_name");
                PointTotal = data.GetDouble("club_point_total");
                PointCurrentSeason = data.GetDouble("club_point_current_season");
            }
        }

        public class ClubBidPrice : IClubBidPrice {
            public int PackageId { get; }
            public float Price { get; }

            public ClubBidPrice(ISFSObject data) {
                PackageId = data.GetInt("package_id");
                Price = data.GetFloat("bid_price");
            }
        }
    }
}