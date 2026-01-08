using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using BLPvpMode.Manager.Api;
using BLPvpMode.Manager.Api.Handlers;

using CustomSmartFox.SolCommands;

using Engine.Utils;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Services;
using Services.Server.Exceptions;

using Sfs2X.Entities.Data;

using UnityEngine;

using IServerDispatcher = App.IServerDispatcher;
using Object = UnityEngine.Object;

namespace PvpMode.Services {
    public partial class DefaultPvpServerBridge : IPvpServerBridge {
        [NotNull]
        private readonly IExtensionRequestBuilder _requestBuilder;

        [NotNull]
        private readonly ITaskDelay _taskDelay;

        private readonly bool _enableLog;
        private readonly IServerDispatcher _serverDispatcher;

        private PvpRankingResult _cachePvpRankingResult;
        private DateTime _lastTimePvpCache;
        private const int MinTimeUpdate = 300;

        public DefaultPvpServerBridge(
            [NotNull] IServerDispatcher serverDispatcher,
            bool enableLog,
            [NotNull] IExtensionRequestBuilder requestBuilder,
            [NotNull] ITaskDelay taskDelay
        ) {
            _serverDispatcher = serverDispatcher;
            _enableLog = enableLog;
            _requestBuilder = requestBuilder;
            _taskDelay = taskDelay;
        }

        public async Task<ISyncPvPConfigResult> SyncPvPConfig() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdSyncPvpConfig(data));
            return new SyncPvPConfigResult(response);
        }

        public async Task<ISyncPvPHeroesResult> SyncPvPHeroes() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdSyncPvpHeroEnergy(data));
            return new SyncPvPHeroesResult(response);
        }

        public async Task JoinQueue(
            int mode,
            string matchId,
            int heroId,
            int[] boosters,
            IPingInfo[] pingInfo,
            int avatarId,
            bool test = false
        ) {
            var data = new SFSObject().Apply(it => {
                var sfsPings = new SFSArray();
                foreach (var ping in pingInfo) {
                    var sfsPing = new SFSObject();
                    sfsPing.PutInt("ping", ping.Latency);
                    sfsPing.PutUtfString("zone_id", ping.ZoneId);
                    sfsPings.AddSFSObject(sfsPing);
                }
                it.PutLong("timestamp", Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond));
                it.PutUtfString("match_id", matchId ?? "");
                it.PutInt("mode", mode);
                it.PutLong("hero_id", heroId);
                it.PutIntArray("boosters", boosters);
                it.PutBool("test", test);
                it.PutSFSArray("pings", sfsPings);
                it.PutInt("avatar", avatarId);
                
                //Dùng để test vào 1 server mong muốn
                if (!AppConfig.IsProduction) {
                    // Để tiện cho test mà ko phải sửa code quá nhiều nên dùng static
                        var zone = TestServerPvp.CurrentZone;
                        if (zone != "none") {
                            var fightBot = TestServerPvp.FightBot;
                            var data = new JObject {
                                ["zone"] = zone,
                                ["vs_bot"] = fightBot,
                            };
                            it.PutUtfString("test_data", data.ToString());
                        }
                }
            });

            await _serverDispatcher.SendCmd(new CmdJoinPvpQueue(data));
        }

        public async Task LeaveQueue() {
            var data = new SFSObject();

            await _serverDispatcher.SendCmd(new CmdLeavePvpQueue(data));
        }

        public async Task<IBoosterResult> GetUserBooster() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetUserPvpBoosters(data));
            return new BoosterResult(response);
        }

        public async Task<IPvpRankingResult> GetPvpRanking(int page = 1, int size = 100) {
            if (_cachePvpRankingResult == null || (DateTime.Now - _lastTimePvpCache).TotalSeconds > MinTimeUpdate) {
                var data = new SFSObject().Apply(it => {
                    it.PutInt("page", page);
                    it.PutInt("page_size", size);
                });
                
                var response = await _serverDispatcher.SendCmd(new CmdGetPvpRanking(data));
                _cachePvpRankingResult = new PvpRankingResult(response);
                _lastTimePvpCache = DateTime.Now;
            }
            return _cachePvpRankingResult;
        }

        public async Task<IPvpOtherUserInfo> GetOtherUserInfo(int userId, string userName) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("user_id", userId);
                it.PutUtfString("user_name", userName);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetOtherUserInfo(data));
            return new PvpOtherUserInfo(response);
        }

        public async Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfig() {
            var coinLeaderboardConfigResult = new List<ICoinLeaderboardConfigResult>();
            var data = new SFSObject();

            var serverManager = (IServerManager)_serverDispatcher;
            var response = await serverManager.SendExtensionRequestAsync(new CmdGetCoinLeaderboardConfig(data));
            var dataArray = response.GetSFSArray("data");
            var configs = JsonConvert.DeserializeObject<CoinLeaderboardConfigResult[]>(dataArray.ToJson()) ??
                          throw new Exception("Parse Leaderboard Config Error");
            foreach (var config in configs) {
                coinLeaderboardConfigResult.Add(config);
            }
            return coinLeaderboardConfigResult.ToArray();
        }

        public async Task<ICoinRankingResult> GetCoinRanking() {
            var data = new SFSObject();

            var serverManager = (IServerManager)_serverDispatcher;
            var response = await serverManager.SendExtensionRequestAsync(new CmdGetCoinRanking(data));
            return new CoinRankingResult(response);
        }

        public async Task<ICoinRankingResult> GetAllSeasonCoinRanking() {
            var data = new SFSObject();

            var serverManager = (IServerManager)_serverDispatcher;
            var response = await serverManager.SendExtensionRequestAsync(new CmdGetAllSeasonCoinRanking(data));
            return new CoinRankingResult(response);
        }

        public async Task<IPvpClaimRewardResult> ClaimPvpReward() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdClaimMonthlyReward(data));
            _cachePvpRankingResult.CurrentReward.IsClaim = true;
            return new PvpClaimRewardResult(response);
        }

        public async Task<IPvpClaimMatchRewardResult> ClaimMatchReward() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdClaimPvpMatchReward(data));
            return new PvpClaimMatchRewardResult(response);
        }

        public async Task<IPvpHistoryResult> GetPvpHistory(int at, int size) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("at", at);
                it.PutInt("count", size);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetPvpHistory(data));
            return new PvpHistoryResult(response);
        }

        public async Task<IOpenChestResult> OpenChest() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdOpenSkinChest(data));
            return new OpenChestResult(response);
        }

        public async Task<IGetEquipmentResult> GetEquipment() {
            var data = new SFSObject();

            var response = await _serverDispatcher.SendCmd(new CmdGetSkinInventory(data));
            return new GetEquipmentResult(response);
        }

        public async Task Equip(int itemType, IEnumerable<(int, long)> itemList) {
            var data = new SFSObject().Apply(it => {
                var items = new SFSArray();
                foreach (var (itemId, expirationAfter) in itemList) {
                    var item = new SFSObject();
                    item.PutInt("item_id", itemId);
                    item.PutLong("expiration_after", expirationAfter);
                    items.AddSFSObject(item);
                }
                it.PutInt("item_type", itemType);
                it.PutSFSArray("items", items);
            });

            await _serverDispatcher.SendCmd(new CmdActiveSkinChest(data));
        }

        public async Task<IBonusRewardPvp> GetBonusRewardPvp(string matchId, string adsId) {
            var data = new SFSObject().Apply(it => {
                it.PutUtfString("reward_id", matchId);
                it.PutUtfString("ads_id", adsId);
            });

            var response = await _serverDispatcher.SendCmd(new CmdGetBonusRewardPvp(data));
            var dataArray = response.GetSFSArray("data");
            var rewards = JsonConvert.DeserializeObject<BonusRewardPvp[]>(dataArray.ToJson()) ??
                          throw new Exception("Parse Bonus Reward Pvp Error");
            foreach (var reward in rewards) {
                if (reward.RewardName != "GOLD") {
                    continue;
                }
                return reward;
            }
            throw new Exception("Parse Bonus Reward Pvp not GOLD");
        }

        private ExtensionResponseHandler BuildRequest(string cmd, ISFSObject data) {
            return new ExtensionResponseHandler(_enableLog, cmd, _requestBuilder.Build(cmd, data), _taskDelay);
        }
    }
}