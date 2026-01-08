using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Senspark;
using Newtonsoft.Json;
using Scenes.TreasureModeScene.Scripts.Solana.Server_Response;
using Scenes.TreasureModeScene.Scripts.Solana.ServerSolDeclaration;
using Server.Models;
using Services;
using Sfs2X.Entities.Data;

namespace App.BomberLand {
    public partial class DefaultGeneralServerBridge {
        private IHeroPower[] OnGetHeroPower(ISFSObject data) {
            var array = data.GetSFSArray(SFSDefine.SFSField.Datas);
            var entries = JsonConvert.DeserializeObject<HeroPower[]>(array.ToJson());
            var result = entries.Select(item => (IHeroPower) item).ToArray();
            _storageManager.LoadUpgradePowerFromServer(result);
            return result;
        }
        
        private ISyncHeroResponse OnSyncHero(ISFSObject data, bool notifyNewIds, bool isBuyHero) {
            var result = new SyncHeroResponse(data);
            _playerStorageManager.LoadPlayerFromServer(result.Details);
            _playerStorageManager.SetTotalHeroesSize(result.HeroesSize);
            if (notifyNewIds && result.NewIds.Length > 0) {
                _serverDispatcher.DispatchEvent(e => e.OnNewHeroFi?.Invoke(result.NewIds, isBuyHero));
            }
            _serverDispatcher.DispatchEvent(observer => observer.OnSyncHero?.Invoke(result));
            return result;
        }
        
        private ISyncHouseResponse OnSyncHouseServer(ISFSObject data) {
            var result = new SyncHouseResponse(data);
            _houseStorageManager.LoadHouseFromServer(result.Details);
            if (result.OldSeasonDetails != null) {
                _houseStorageManager.LoadLockedHousesFromServer(result.OldSeasonDetails);
            }
            _houseStorageManager.LoadHeroInHouseFromServer(result.HeroInHouse);
            _serverDispatcher.DispatchEvent(observer => observer.OnSyncHouse?.Invoke(result));
            return result;
        }
        
        private IChestReward OnGetChestReward(ISFSObject data) {
            var reward = ParseChestReward(data);
            ParseCurrentMiningToken(data);
            return reward;
        }
        
        private bool OnReactiveHouse(ISFSObject data) {
            ParseChestReward(data);
            return true;
        }
        
        private IApproveClaimResponse OnApproveClaim(ISFSObject data) {
            ParseChestReward(data);
            var result = _claimTokenServerBridge.OnApproveClaim(data);
            return result;
        }
        
        private float OnConfirmApproveClaimSuccess(ISFSObject data) {
            ParseChestReward(data);
            var received = (float) data.GetDouble("received");
            return received;
        }
        
        private IChestReward OnSyncDeposited(ISFSObject data) {
            var result = ParseChestReward(data);
            return result;
        }
        
        private IAutoMinePackages OnGetAutoMinePrice(ISFSObject data) {
            var packageDetail = new AutoMinePackage(data);
            _storageManager.AutoMinePackages = packageDetail.Packages;
            _storageManager.AutoMineInfo = packageDetail.Info;
            return packageDetail;
        }
        
        private IChestReward OnBuyAutoMine(ISFSObject data) {
            var info = new AutoMineInfo(data);
            var chestReward = ParseChestReward(data);
            _storageManager.AutoMineInfo = info;
            return chestReward;
        }
        
        private IRockPackage OnBuyRockPack(ISFSObject data) {
            var info = new RockPackage(data);
            ParseChestReward(data);
            return info;
        }
        
        private bool OnStartAutoMine(ISFSObject data) {
            var info = new AutoMineInfo(data);
            _storageManager.AutoMineInfo = info;
            return info.ActiveAutoMine;
        }
        
        private bool OnRepairShield(ISFSObject data) {
            var heroData = data.GetSFSObject("repaired_hero");
            var chestReward = ParseChestReward(data);
            var result = HeroDetails.Parse(heroData);
            var heroId = new HeroId(result.Id, result.AccountType);
            _playerStorageManager.UpdatePlayerHpFromServer(result);
            _playerStorageManager.UpdateHeroSShield(heroId, result.HeroSAbilities);
            _playerStorageManager.UpdateHeroState(heroId, result.Stage);
            _playerStorageManager.UpdateHeroActiveState(heroId, result.IsActive);
            return true;
        }
        
        private bool OnChangeMiningToken(ISFSObject data) {
            ParseCurrentMiningToken(data);
            return true;
        }
        
        private void ParseCurrentMiningToken(ISFSObject data) {
            var token = new MiningTokenData(data);
            if (token.IsValid) {
                _storageManager.MiningTokenType = token.TokenType;
            }
        }
        
        private IAirDropResponse OnGetAirDrop(ISFSObject data) {
            var array = data.GetSFSArray("data");
            var result = new AirDropResponse(array);
            return result;
        }

        private IAirDropClaimResponse OnClaimAirDrop(ISFSObject data) {
            var result = new AirDropClaimResponse(data);
            ParseChestReward(data);
            return result;
        }

        private bool OnConfirmClaimAirDrop(ISFSObject data) {
            return true;
        }
        
        private bool OnUserRename(ISFSObject data) {
            ParseChestReward(data);
            return true;
        }

        private bool OnLinkUser(ISFSObject data) {
            var result = data.GetBool("result");
            return result;
        }
        
        private double OnUserStake(ISFSObject data) {
            var myStaked = data.GetDouble("total_staked");
            ParseChestReward(data);
            return myStaked;
        }
        
        private IStakeResult OnUserWithdrawStake(ISFSObject data) {
            var date = data.GetLong("stake_date");
            var result = new StakeResult() {
                MyStake = data.GetDouble("principal"),
                WithdrawFee = data.GetDouble("withdraw_fee"),
                DPY = data.GetDouble("apd"),
                Profit = data.GetDouble("profit"),
                ReceiveAmount = data.GetDouble("receive_amount"),
                StakeDate = date > 0 ? DateTimeOffset.FromUnixTimeSeconds(date).DateTime : DateTime.MinValue,
                TotalStake = data.GetDouble("total_stake"),
            };
            ParseChestReward(data);
            return result;
        }
        
        private IVipStakeResponse OnUserStakeVip(ISFSObject data) {
            var result = ParseUserStakeVipResult(data);
            return result;
        }
        
        private IVipStakeResponse OnUserClaimStakeVip(ISFSObject data) {
            var result = ParseUserStakeVipResult(data);
            return result;
        }
        
        private IVipStakeResponse ParseUserStakeVipResult(ISFSObject data) {
            var result = new VipStakeResponse(data);
            _storageManager.VipStakeResults = result;
            return result;
        }
        
        private ILuckyRewardsResponse OnStartLuckyWheel(ISFSObject data) {
            var result = new LuckyRewardsResponse(data);
            return result;
        }

        private IDailyMissionListResponse OnGetDailyMissions(ISFSObject data) {
            var result = new DailyMissionListResponse(data);
            return result;
        }

        private bool OnClaimLuckyTicketDailyMission(ISFSObject data) {
            return true;
        }

        private IEmailResponse OnGetEmail(ISFSObject data) {
            var result = new EmailResponse(data);
            return result;
        }

        private bool OnRegisterEmail(ISFSObject data) {
            return true;
        }

        private bool OnVerifyEmail(ISFSObject data) {
            return true;
        }
        
        private IChestReward OnBuyTicket(ISFSObject data) {
            var reward = ParseChestReward(data);
            return reward;
        }

        private bool OnBuyLuckyTicket(ISFSObject data) {
            return true;
        }
        
        private IServerConfigResponse OnGetConfig(ISFSObject data) {
            var productManager = ServiceLocator.Instance.Resolve<IProductManager>();
            var result = new ServerConfigResponse(data);
            productManager.Initialize(result.Products);
            _storageManager.SetSessionData(SessionKey.CanBuyHeroesTrial, result.CanBuyHeroesTrial);
            ServiceLocator.Instance.Resolve<IHeroColorManager>().Initialize(result.Heroes.Select(it => new HeroColorData(
                it.Color,
                it.Skin
            )));
            ServiceLocator.Instance.Resolve<IItemUseDurationManager>().Initialize(result.SkinItemExpiryTime);
            ServiceLocator.Instance.Resolve<IEarlyConfigManager>().Initialize(data.ToJson());
            return result;
        }
        
        private float OnPreviewOrSwapToken(ISFSObject data) {
            var result = data.GetFloat("data");
            return result;
        }
        
        private int OnGetSwapTokenConfig(ISFSObject data) {
            var result = data.GetInt("min_gem_swap");
            return result;
        }
        
        private IMinStakeHeroManager OnGetMinStakeManager(ISFSObject data) {
            var result = new MinStakeHeroManager(data);
            _storageManager.MinStakeHero = result;
            return result;
        }
        
        private IRepairShieldConfig OnGetRepairShieldConfig(ISFSObject data) {
            var result = new RepairShieldConfig(data);
            _storageManager.RepairShieldConfig = result;
            return result;
        }
        
        private IRockPackConfigs OnGetRockPackConfig(ISFSObject data) {
            var result = new RockPackConfigs(data);
            _storageManager.RockPackConfigs = result;
            return result;
        }

        private void OnGetRentHousePackageConfig(ISFSObject data) {
            var result = new RentHousePackageConfigs(data);
            _storageManager.RentHousePackConfigs = result;
        }
        
        private ITreasureHuntConfigResponse OnGetTreasureHuntDataConfig(ISFSObject data) {
            var result = new TreasureHuntConfigResponse(data);
            
            _storageManager.HeroLimit = result.HeroLimit;
            _storageManager.HeroPrice = result.HeroPrice;
            _storageManager.UpgradePrice = result.HeroUpgradePrice;
            _storageManager.HeroRandomizeAbilityCost = result.HeroAbilityDesign;
            _storageManager.HouseLimit = result.HouseLimit;
            _storageManager.HousePrice = result.HousePrices;
            _storageManager.HouseMintLimits = result.HouseMintLimits;
            _storageManager.Charge = result.HouseStats.Select(item => item.Recovery / 60f).ToArray();
            _storageManager.Slot = result.HouseStats.Select(item => item.Capacity).ToArray();
            _storageManager.FusionFee = result.FusionFee;
            _storageManager.HousePriceTokenNetwork = result.HousePriceTokenNetwork;
            _storageManager.EndTimeTokenNetwork = result.EndTimeTokenNetwork;
            
            return result;
        }
        
        private IBurnHeroConfig OnGetBurnHeroConfig(ISFSObject data) {
            var result = new BurnHeroConfig(data);
            _storageManager.BurnHeroConfig = result;
            return result;
        }
        
        private void OnBurnHero(ISFSObject data) {
            _storageManager.LastBurnHeroData = null;
            var code = data.GetInt("code");
            if (code == 0) {
                ParseChestReward(data);
            }
        }
        
        private IUpgradeShieldConfig OnGetUpgradeShieldConfig(ISFSObject data) {
            var result = new UpgradeShieldConfig(data);
            _storageManager.UpgradeShieldConfig = result;
            return result;
        }
        
        private UpgradeShieldResponse OnUpgradeLevelShield(ISFSObject data) {
            ParseChestReward(data);
            var result = new UpgradeShieldResponse(data);
            return result;
        }
        
        private IChestReward ParseChestReward(ISFSObject data) {
            var rewards = new List<ITokenReward>();
            if (!data.ContainsKey("rewards")) {
                return null;
            }
            var array = data.GetSFSArray("rewards");
            for (var i = 0; i < array.Size(); ++i) {
                var item = new TokenReward(array.GetSFSObject(i));
                rewards.Add(item);
            }
            var result = new ChestReward(rewards);
            _chestRewardManager.InitNewChestReward(result);
            _serverDispatcher.DispatchEvent(observer => observer.OnChestReward?.Invoke(result));
            return result;
        }
        
        private IBuyHeroServerResponse OnBuyHeroServer(ISFSObject data, bool isBuyHero) {
            ParseChestReward(data);
            
            var result = new BuyHeroSolResponse(data);
            var newIds = result.Details.Select(iter => iter.Id).ToArray();
            _playerStorageManager.AddHeroServer(result.Details);
            _serverDispatcher.DispatchEvent(e => e.OnNewHeroServer?.Invoke(newIds, isBuyHero));
            var syncHeroResponse = new SyncHeroResponse(result.Details);
            _serverDispatcher.DispatchEvent(observer => observer.OnSyncHero?.Invoke(syncHeroResponse));
            //Kiểm tra số lượng hero đang có để hoàn thành task mua hero
            _taskTonManager.CheckBuyHeroTask();
            return result;
        }
        
        private IHouseDetails OnBuyHouseServer(ISFSObject data) {
            var result = HouseDetails.Parse(data);
            ParseChestReward(data);
            return result;
        }
        
        private IOfflineReward OnGetOfflineReward(ISFSObject data) {
            var result = new OnOfflineReward(data);
            if (result.amount > 0)
                ParseChestReward(data);
            return result;
        }
        
        private ReferralData OnGetReferralData(ISFSObject data) {
            var referralData = new ReferralData(data);
            return referralData;
        }

        private IClubInfo OnGetClubInfo(ISFSObject data) {
            if (!data.ContainsKey("club_id")) {
                return null;
            }
            
            var clubInfo = new ClubInfo(data);
            return clubInfo;
        }
        
        private IClubInfo OnJoinClubInfo(ISFSObject data) {
            if (!data.ContainsKey("club_id")) {
                return null;
            }
            
            var clubInfo = new ClubInfo(data);
            _serverDispatcher.DispatchEvent(e => e.OnJoinClub?.Invoke(clubInfo));
            return clubInfo;
        }
        
        private IClubInfo OnLeaveClubInfo(ISFSObject data) {
            _serverDispatcher.DispatchEvent(e => e.OnLeaveClub?.Invoke(null));
            return null;
        }
        
        private IClubRank[] OnGetListClubRank(ISFSObject data) {
            var clubsArray = data.GetSFSArray("data");
            var result = new IClubRank[clubsArray.Count];
            for (var i = 0; i < clubsArray.Count; i++) {
                result[i] = new ClubRank(clubsArray.GetSFSObject(i));
            }
            return result;
        }
        
        private IClubBidPrice[] OnGetBidPrice(ISFSObject data) {
            var packagesArray = data.GetSFSArray("data");
            var result = new IClubBidPrice[packagesArray.Count];
            for (var i = 0; i < packagesArray.Count; i++) {
                result[i] = new ClubBidPrice(packagesArray.GetSFSObject(i));
            }
            return result;
        }
        
        private string OnDepositSolana(ISFSObject data) {
            var invoice = data.GetUtfString("invoice");
            return invoice;
        }
        
        private ISyncHeroResponse OnSyncHeroServer(ISFSObject data) {
            var result = new SyncHeroServerResponse(data);
            _playerStorageManager.LoadPlayerFromServer(result.Details);
            if (result.HeroesSize == -1) {
                _playerStorageManager.SetTotalHeroesSize(result.Details.Length);
            } else {
                _playerStorageManager.SetTotalHeroesSize(result.HeroesSize);
            }
            _serverDispatcher.DispatchEvent(observer => observer.OnSyncHero?.Invoke(result));
            return result;
        }
        
        private IFusionTonHeroResponse OnFusionServerHero(ISFSObject data) {
            ParseChestReward(data);
            var result = new FusionTonHeroResponse(data);
            _playerStorageManager.LoadPlayerFromServer(result.Details);
            if (result.Result) {
                _serverDispatcher.DispatchEvent(e => e.OnNewHeroServer?.Invoke(result.NewIds, false));
            }
            return result;
        }
        
        private IFusionTonHeroResponse OnMultiFusionServerHero(ISFSObject data) {
            ParseChestReward(data);
            var result = new FusionTonHeroResponse(data);
            _playerStorageManager.LoadPlayerFromServer(result.Details);
            if (result.Result) {
                _serverDispatcher.DispatchEvent(e => e.OnNewHeroServer?.Invoke(result.NewIds, false));
            }
            return result;
        }
        
        private void OnGetHeroOldSeason(ISFSObject data) {
            var detailsArray = data.GetSFSArray(SFSDefine.SFSField.OldSeason);
            var details = new IHeroDetails[detailsArray.Size()];
            for (var i = 0; i < detailsArray.Size(); ++i) {
                var entry = detailsArray.GetSFSObject(i);
                details[i] = HeroDetails.Parse(entry);
            }
            _playerStorageManager.LoadLockedHeroesFromServer(details);
        }
        
        private void OnGetOnBoardingConfig(ISFSObject data) {
            var currentStep = data.GetInt(SFSDefine.SFSField.CurrentStep);
            var config = data.GetSFSArray(SFSDefine.SFSField.Config);
            var rewardConfig = new Dictionary<TutorialClaimed, float>();
            for (var i = 0; i < config.Size(); ++i) {
                var reward = config.GetSFSObject(i);
                var rewardValue = reward.GetFloat("reward");
                var idValue = reward.GetInt("id");
                rewardConfig.Add((TutorialClaimed)idValue, rewardValue);
            }
            _onBoardingManager.InitCurrentStep(currentStep);
            _onBoardingManager.InitRewardConfig(rewardConfig);
        }

        private void OnUpdateUserOnBoarding(ISFSObject data) {
            ParseChestReward(data);
        }

        private long OnRentHouse(ISFSObject data) {
            ParseChestReward(data);
            var endTimeRent = data.GetLong("end_time_rent");
            return endTimeRent;
        }
        
        private string OnDepositRon(ISFSObject data) {
            var invoice = data.GetUtfString("invoice");
            return invoice;
        }
    }
}