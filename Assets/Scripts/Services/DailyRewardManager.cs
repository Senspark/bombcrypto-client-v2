using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using App;

using Data;

using Senspark;

using Newtonsoft.Json;

using Services.IapAds;
using Services.Server.Exceptions;

using Utils;

namespace Services {
    public class DailyRewardManager : IDailyRewardManager {
        private const string RandomInList = "RANDOM_IN_LIST";

        private class GetDailyMissionData {
            [JsonProperty("id")]
            public string Id;

            [JsonProperty("mission_code")]
            public string MissionCode;

            [JsonProperty("number_mission")]
            public int NumberMission;

            [JsonProperty("completed_mission")]
            public int CompletedMission;

            [JsonProperty("cool_down_end_at")]
            public long CoolDownEndAt;

            [JsonProperty("rewards")]
            public GetDailyMissionRewardData[] Rewards;
        }

        private class GetDailyMissionRewardData {
            [JsonProperty("item_type")]
            public string ItemType;

            [JsonProperty("type")]
            public string Type;
            
            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("rewards")]
            public GetDailyMissionRewardItemData[] Rewards;
        }

        private class GetDailyMissionRewardItemData {
            [JsonProperty("item_id")]
            public int Id;

            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("type")]
            public string Type;
        }

        private class TakeDailyMissionRewardsData {
            [JsonProperty("rewards")]
            public TakeDailyMissionRewardsRewardData[] Rewards;

            [JsonProperty("items")]
            public TakeDailyMissionRewardsItemData[] Items;
        }

        private class TakeDailyMissionRewardsRewardData {
            [JsonProperty("reward_type")]
            public string RewardType;

            [JsonProperty("quantity")]
            public int Quantity;
        }

        private class TakeDailyMissionRewardsItemData {
            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("quantity")]
            public int Quantity;
        }

        private IUnityAdsManager _adsManager;
        private readonly IAnalytics _analytics;
        private GetDailyMissionData[] _data;
        private int _level;
        private readonly IServerRequester _serverRequester;

        public DailyRewardManager(IAnalytics analytics, IServerRequester serverRequester) {
            _analytics = analytics;
            _serverRequester = serverRequester;
        }

        public async Task<IEnumerable<(int, string, int)>> ClaimRewardAsync() {
            var data = _data[_level];
            _adsManager ??= ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            var token = await _adsManager.ShowRewarded();
            var watchDailyMissionAds = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.WatchDailyMission(data.MissionCode, token)
            );
            if (watchDailyMissionAds.Code != 0) {
                throw new Exception(watchDailyMissionAds.Message);
            }
            var takeDailyMissionResult =
                JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<TakeDailyMissionRewardsItemData[]>>(
                    await _serverRequester.TakeDailyMission(data.MissionCode)
                );
            if (takeDailyMissionResult.Code != 0) {
                throw new Exception(takeDailyMissionResult.Message);
            }
            _level++;
            _analytics.DailyGift_TrackCollectItems(_level);
            var result = takeDailyMissionResult.Data.Select(it => (
                it.ItemId,
                string.Empty,
                it.Quantity
            )).ToList();

            //Reset data sau khi claim để có thể UpdateDataAsync sau đó.
            ResetData();

            return result;
        }

        public void Destroy() {
        }

        private IEnumerable<DailyRewardData> GetDailyRewards() {
            var result = new List<DailyRewardData>();
            if(_data == null)
                return result;
            for (var index = 0; index < _data.Length; index++) {
                var it = _data[index];
                var claimTime = DateTime.UnixEpoch + TimeSpan.FromMilliseconds(it.CoolDownEndAt);
                var items = new List<DailyRewardItemData>();
                foreach (var r1 in it.Rewards) {
                    if (r1.Type == "TAKE_ALL_IN_LIST") {
                        foreach (var r2 in r1.Rewards) {
                            items.Add(new DailyRewardItemData() {
                                Id = r2.Id,
                                Quantity = r2.Quantity,
                                Type = r2.Type
                            });
                        }
                        
                    } else if (r1.Type == "RANDOM_ALL") {
                        items.Add(new DailyRewardItemData() {
                            Id = 0,
                            Quantity = r1.Quantity,
                            Type = r1.ItemType
                        });
                    } else if (r1.Type == "RANDOM_IN_LIST") {
                        foreach (var r2 in r1.Rewards) {
                            items.Add(new DailyRewardItemData() {
                                Id = r2.Id,
                                Quantity = r2.Quantity,
                                Type = r2.Type
                            });
                        }
                    }
                }
                var data = new DailyRewardData {
                    ClaimTime = claimTime,
                    // Items = it.Reward.Rewards.Select(item => (item.Id, item.Quantity, item.Type)).ToArray(),
                    Items = items.ToArray(),
                    Randomize = it.Rewards[0].Type.Contains("RANDOM"),
                    //Randomize = false,
                    Status = GetStatus(claimTime, index),
                    // RandomIcon = it.Reward.ItemType
                    RandomIcon = ""
                };
                result.Add(data);
            }
            return result;
        }

        IEnumerator<DailyRewardData> IEnumerable<DailyRewardData>.GetEnumerator() {
            return GetDailyRewards().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetDailyRewards().GetEnumerator();
        }

        private DailyRewardData.DailyRewardStatus GetStatus(DateTime claimTime, int index) {
            if (index < _level) {
                return DailyRewardData.DailyRewardStatus.Claimed;
            }
            if (index > _level) {
                return DailyRewardData.DailyRewardStatus.Locked;
            }
            return (DateTime.UtcNow - claimTime).Ticks > 0
                ? DailyRewardData.DailyRewardStatus.Countdown
                : DailyRewardData.DailyRewardStatus.None;
        }

        Task<bool> IService.Initialize() {
            return Task.FromResult(true);
        }

        private void ResetData() {
            _data = null;
        }

        public async Task UpdateDataAsync() {
            #if UNITY_WEBGL
            _data = null;
#else
            // Kiểm tra data để chỉ chạy Update 1 lần.
            // nếu muốn UpdateDataAsync lại thì reset data
            if (_data != null) {
                return;
            }
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<GetDailyMissionData[]>>(
                await _serverRequester.GetDailyMission()
            );
            if (result.Code == 0) {
                _data = result.Data;
                _level = Array.FindIndex(_data, it => it.CompletedMission < it.NumberMission);
            } else {
                throw new ErrorCodeException(result.Code, result.Message);
            }
#endif
        }
    }
}