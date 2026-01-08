using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using App;
using Data;
using Senspark;
using Newtonsoft.Json;
using PvpMode.Utils;
using Services;
using Sfs2X.Entities.Data;
using Ton.Leaderboard;

namespace PvpMode.Services {
    public partial class DefaultPvpServerBridge {
        private class PvPHeroEnergy : IPvPHeroEnergy {
            public int Balance { get; }
            public long Id { get; }
            public long RemainingTime { get; }

            public PvPHeroEnergy(ISFSObject data) {
                Balance = data.GetInt("balance");
                Id = data.GetLong("id");
                RemainingTime = data.GetLong("remaining_time") / 1000;
            }
        }

        private class SyncPvPConfigResult : ISyncPvPConfigResult {
            public int[] Bets { get; }
            public long EventExp { get; }
            public int OpenChestRequiredSharp { get; }
            public float RewardFee { get; }
            public bool SeasonValid { get; }
            public int TicketPrice { get; }
            public bool IsWhitelist { get; }
            public IDictionary<int, string> Items { get; }

            public SyncPvPConfigResult(ISFSObject parameters) {
                var data = parameters.GetSFSObject("data");
                Bets = data.GetIntArray("bets");
                EventExp = data.GetLong("end_time_event_nft_pvp");
                OpenChestRequiredSharp = data.GetInt("pvp_number_shard");
                RewardFee = data.GetFloat("reward_fee");
                SeasonValid = data.GetBool("season_valid");
                TicketPrice = data.GetInt("ticket_price");
                IsWhitelist = data.GetBool("is_white_list");
                Items = new Dictionary<int, string>();
                var items = data.GetSFSArray("product_items") ?? new SFSArray();
                for (var i = 0; i < items.Count; i++) {
                    var item = items.GetSFSObject(i);
                    Items[item.GetInt("item_id")] = item.GetUtfString("name");
                }
            }
        }

        private class SyncPvPHeroesResult : ISyncPvPHeroesResult {
            public IPvPHeroEnergy[] HeroEnergies { get; }
            public long LastPlayedHero { get; }
            public int LastBet { get; }
            public IDictionary<int, int> Equipments { get; }

            public SyncPvPHeroesResult(ISFSObject data) {
                var items = data.GetSFSArray("items");
                HeroEnergies = new IPvPHeroEnergy[items.Count];
                for (var i = 0; i < items.Count; i++) {
                    HeroEnergies[i] = new PvPHeroEnergy(items.GetSFSObject(i));
                }
                LastPlayedHero = data.GetLong("last_played_hero");
                LastBet = data.GetInt("last_bet");
                Equipments = new Dictionary<int, int>();
                var equipments = data.GetSFSArray("equipments") ?? new SFSArray();
                for (var i = 0; i < equipments.Count; i++) {
                    var equipment = equipments.GetSFSArray(i);
                    Equipments[equipment.GetInt(0)] = equipment.GetInt(1);
                }
            }
        }
        
        public class PvpGenericResult : IPvpGenericResult {
            public int Code { get; }
            public string Message { get; }

            public static IPvpGenericResult FastParse(ISFSObject data) {
                var code = data.GetInt("code");
                var message = data.GetUtfString("message");
                return new PvpGenericResult(code, message);
            }

            public static IPvpGenericResult Parse(ISFSObject data) {
                var json = data.ToJson();
                var result = JsonConvert.DeserializeObject<PvpGenericResult>(json);
                return result;
            }

            public PvpGenericResult(
                [JsonProperty("code")] int code,
                [JsonProperty("message")] string message) {
                Code = code;
                Message = message;
            }
        }

        private class BoosterResult : IBoosterResult {
            public Manager.IBooster[] Boosters { get; }

            public BoosterResult(ISFSObject data) {
                Boosters = Manager.DefaultBoosterManager.ParseBoosters(data);
            }
        }

        private class PvpRankingItemResult : IPvpRankingItemResult {
            public int UserId { get; }
            public int WinMatch { get; }
            public int TotalMatch { get; }
            public int RankNumber { get; }
            public string Name { get; }
            public string UserName { get; }
            public int Point { get; }
            public int Avatar { get; }
            public int BombRank { get; }
            public RewardData[] Rewards { get; }

            public PvpRankingItemResult(ISFSObject data, IPvPRankRewardManager rewardManager) {
                UserId = data.GetInt("user_id");
                WinMatch = data.GetInt("win_match");
                TotalMatch = data.GetInt("total_match");
                RankNumber = data.GetInt("rank_number");
                Name = data.GetUtfString("name");
                UserName = data.GetUtfString("user_name");
                Point = data.GetInt("point");
                Avatar = data.GetInt("avatar");
                Rewards = rewardManager.GetRewards(RankNumber);
                BombRank = data.GetInt("bomb_rank");
            }
            
            public PvpRankingItemResult(ISFSObject data) {
                WinMatch = data.GetInt("win_match");
                TotalMatch = data.GetInt("total_match");
                RankNumber = data.GetInt("rank_number");
                Name = data.GetUtfString("name");
                Point = data.GetInt("point");
                Avatar = data.GetInt("avatar");
                BombRank = data.GetInt("bomb_rank");
            }
        }

        private class PvpRankingResult : IPvpRankingResult {
            public int TotalCount { get; }
            public int RemainTime { get; }
            public IPvpRankingItemResult[] RankList { get; }
            public IPvpRankingItemResult CurrentRank { get; }
            public IPvpCurrentRewardResult CurrentReward { get; }
            public int CurrentSeason { get; }
            public bool SeasonValid { get; }

            public PvpRankingResult(ISFSObject data) {
                var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
                logManager.Log(data.ToJson());
                var rewardManager = new PvPRankRewardManager(
                    JsonConvert.DeserializeObject<PvPRankRewardData[]>(data.GetSFSArray("pvp_ranking_reward")
                        .ToJson()));
                TotalCount = data.GetInt("total_count");
                RemainTime = data.GetInt("remain_time");
                var current = data.GetSFSObject("current_rank");
                logManager.Log($"current: {current.ToJson()}");
                CurrentRank = new PvpRankingItemResult(current, rewardManager);
                var rankList = data.GetSFSArray("rank_list");
                RankList = new IPvpRankingItemResult[rankList.Count];
                for (var i = 0; i < rankList.Count; i++) {
                    RankList[i] = new PvpRankingItemResult(rankList.GetSFSObject(i), rewardManager);
                }
                SeasonValid = data.GetBool("pvp_season_valid");
                var rankReward = data.GetSFSObject("reward").GetSFSObject("rank_reward");
                logManager.Log($"rankReward: {rankReward.ToJson()}");
                CurrentReward = new PvpCurrentRewardResult(rankReward);
                CurrentSeason = data.GetInt("pvp_current_season");
            }
        }

        private class PvpOtherUserInfo : IPvpOtherUserInfo {
            public IPvpRankingItemResult Rank { get; }
            public EquipmentData[] EquipData { get; }
            public PlayerData Hero { get; }

            public PvpOtherUserInfo(ISFSObject data) {
                Rank = new PvpRankingItemResult(data.GetSFSObject("rank"));
                Hero = DefaultPlayerStoreManager.GenerateOtherPlayerData(data.GetSFSObject("hero"));

                var equipments = data.GetSFSArray("equip_items");
                EquipData = new EquipmentData[equipments.Count];
                for (var i = 0; i < equipments.Count; i++) {
                    EquipData[i] = EquipmentData.ParseOtherUserInfo(equipments.GetSFSObject(i));
                }
            }
        }
        
        private class CoinLeaderboardConfigResult : ICoinLeaderboardConfigResult {
            public string Name { get; }
            public int Rank { get; }
            public int UpRankPointUser { get; }
            public long UpRankPointClub { get; }
            
            [JsonConstructor]
            public CoinLeaderboardConfigResult(int rank, string name, int up_rank_point_user, long up_rank_point_club) {
                Rank = rank;
                Name = name;
                UpRankPointUser = up_rank_point_user;
                UpRankPointClub = up_rank_point_club;
            }
        }
        
        public class RankTypeInfo {
            public AirdropRankType rankType;
            public int startPointUser;
            public int endPointUser;
            public long startPointClub;
            public long endPointClub;

            public RankTypeInfo(ICoinLeaderboardConfigResult[] config, AirdropRankType type) {
                rankType = type;
                startPointUser = type == AirdropRankType.Bronze ? 0 : config[(int)type - 1].UpRankPointUser;
                endPointUser = config[(int)type].UpRankPointUser;
                startPointClub = type == AirdropRankType.Bronze ? 0 : config[(int)type - 1].UpRankPointClub;
                endPointClub = config[(int)type].UpRankPointClub;
            }
        }
        
        private class CoinRankingItemResult : ICoinRankingItemResult {
            public int RankNumber { get; set; }
            public string Name { get; }
            public float Point { get; }

            public CoinRankingItemResult(ISFSObject data) {
                RankNumber = data.GetInt("rank");
                Name = data.GetUtfString("name");
                Point = (float)data.GetDouble("coin");
            }
        }
        
        private class CoinRankingResult : ICoinRankingResult {
            public int RemainTime { get; }
            public ICoinRankingItemResult[] RankList { get; }
            public ICoinRankingItemResult CurrentRank { get; }

            public CoinRankingResult(ISFSObject data) {
                var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
                logManager.Log(data.ToJson());
                RemainTime = data.GetInt("remain_time");
                var current = data.GetSFSObject("current_rank");
                logManager.Log($"current: {current.ToJson()}");
                CurrentRank = new CoinRankingItemResult(current);
                var rankList = data.GetSFSArray("rank_list");
                RankList = new ICoinRankingItemResult[rankList.Count];
                for (var i = 0; i < rankList.Count; i++) {
                    RankList[i] = new CoinRankingItemResult(rankList.GetSFSObject(i));
                }
            }
        }
        
        private class PvpRewardResult : IPvpRewardResult {
            public double BCoin { get; }
            public int HeroBox { get; }
            public int Shield { get; }
            public int MinRank { get; }
            public int MaxRank { get; }

            public PvpRewardResult(ISFSObject data) {
                BCoin = data.GetDouble("bcoin");
                HeroBox = data.GetInt("box_hero");
                Shield = data.GetInt("shield");
                MinRank = data.GetInt("rank_min");
                MaxRank = data.GetInt("rank_max");
            }
        }

        private class PvpCurrentRewardResult : IPvpCurrentRewardResult {
            public int Rank { get; }
            public Dictionary<string, int> Reward { get; }
            public double BCoin { get; private set; }
            public int HeroBox { get; private set; }
            public int Shield { get; private set; }
            public bool IsReward { get; private set; }
            public bool IsClaim { get; set; }
            public int TotalMatch { get; }
            public int PvpMatchReward { get; }

            public PvpCurrentRewardResult([NotNull] ISFSObject data) {
                var reward = data.GetSFSObject("reward");
                if (reward != null) {
                    var json = reward.ToJson();
                    ServiceLocator.Instance.Resolve<ILogManager>().Log(json);
                    Reward = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                }
                Rank = data.GetInt("rank_number");
                BCoin = data.GetDouble("bcoin");
                HeroBox = data.GetInt("box_hero");
                Shield = data.GetInt("shield");
                IsReward = BCoin > 0 || HeroBox > 0 || Shield > 0;
                IsClaim = data.GetInt("is_claim") == 1;
                TotalMatch = data.GetInt("total_match");
                PvpMatchReward = data.GetInt("pvp_match_reward");
            }
        }

        private class PvpClaimRewardResult : IPvpClaimRewardResult {
            public IPvpCurrentRewardResult CurrentReward { get; }

            public PvpClaimRewardResult(ISFSObject data) {
                var current = data.GetSFSObject("rank_reward");
                CurrentReward = new PvpCurrentRewardResult(current);
            }
        }

        private class PvpClaimMatchRewardResult : IPvpClaimMatchRewardResult {
            public string RewardId { get; }
            public bool IsOutOfChest { get; }

            public PvpClaimMatchRewardResult(ISFSObject data) {
                RewardId = data.GetUtfString("reward_id");
                IsOutOfChest = data.GetBool("is_out_of_chest_slot");
            }
        }

        private class PvpHistoryItemResult : IPvpHistoryItemResult {
            public string MatchId { get; }
            public string OpponentName { get; }
            public string Opponent { get; }
            public string Time { get; }
            public string Date { get; }
            public bool IsWin { get; }

            public PvpHistoryItemResult(ISFSObject data) {
                MatchId = data.GetUtfString("matchId");
                OpponentName = data.GetUtfString("opponentName");
                Opponent = Ellipsis.EllipsisAddress(data.GetUtfString("opponent"));

                var date = data.GetLong("date");
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(date);
                Time = dateTime.ToString("HH:mm");
                Date = dateTime.ToString("dd/MM/yyyy");

                IsWin = data.GetBool("win");
            }
        }

        private class PvpHistoryResult : IPvpHistoryResult {
            public IPvpHistoryItemResult[] HistoryList { get; }

            public PvpHistoryResult(ISFSObject data) {
                var historyList = data.GetSFSArray("history");
                HistoryList = new IPvpHistoryItemResult[historyList.Count];
                for (var i = 0; i < historyList.Count; i++) {
                    HistoryList[i] = new PvpHistoryItemResult(historyList.GetSFSObject(i));
                }
            }
        }

        public class GetEquipmentResult : IGetEquipmentResult {
            public EquipmentData[] Equipments { get; }

            public GetEquipmentResult(ISFSObject parameter) {
                var equipments = parameter.GetSFSArray("userInventoryNFT");
                Equipments = new EquipmentData[equipments.Count];
                for (var i = 0; i < equipments.Count; i++) {
                    Equipments[i] = EquipmentData.Parse(
                        equipments.GetSFSObject(i)
                    );
                }
            }
        }

        public class OpenChestResult : IOpenChestResult {
            public EquipmentData[] Items { get; }

            public OpenChestResult(ISFSObject parameters) {
                var data = parameters.GetSFSArray("data");
                Items = new EquipmentData[data.Count];
                for (var i = 0; i < data.Count; i++) {
                    var obj = data.GetSFSObject(i);
                    throw new NotImplementedException();
                    // Items[i] = new EquipmentData(false, obj.GetInt("item_id"), obj.GetInt("type"),
                    //     obj.GetInt("id"));
                }
            }
        }
        
        private class BonusRewardPvp : IBonusRewardPvp {
            public string RewardName { get; }
            public string Type { get; }
            public int Value { get; }

            [JsonConstructor]
            public BonusRewardPvp(string reward_type, string type, float value) {
                RewardName = reward_type;
                Type = type;
                Value = (int) value;
            }
        }
    }
}