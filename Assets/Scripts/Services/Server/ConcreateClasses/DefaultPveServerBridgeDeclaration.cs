using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using Server.Models;

using Sfs2X.Entities.Data;

using UnityEngine;

using RewardType = Constant.RewardType;

namespace App {
    public partial class DefaultPveServerBridge {
        private class MapDetails : IMapDetails {
            public int Tileset { get; }
            public IMapBlock[] Blocks { get; }

            private MapDetails(int tileSet, IMapBlock[] blocks) {
                Tileset = tileSet;
                Blocks = blocks;
            }

            public MapDetails(int tileSet, string jsonData) {
                Tileset = tileSet;
                Blocks = ParseBlockData(jsonData);
            }

            private static IMapBlock[] ParseBlockData(string json) {
                var blockJson = JArray.Parse(json);
                var blocks = new IMapBlock[blockJson.Count];
                for (var i = 0; i < blockJson.Count; ++i) {
                    blocks[i] = new MapBlock((JObject) blockJson[i]);
                }
                return blocks;
            }
        }
        
        private class MapBlock : IMapBlock {
            public Vector2Int Position { get; }
            public int Type { get; }
            public float Health { get; }
            public float MaxHealth { get; }
            public ITokenReward[] Rewards { get; }

            public MapBlock(JObject data) {
                Position = new Vector2Int {
                    x = (int) data["i"],
                    y = (int) data["j"],
                };
                Type = (int) data["type"];
                Health = (float) data["hp"];
                MaxHealth = (float) data["maxHp"];

                var rewardJson = (JArray) data["rewards"];
                if (rewardJson == null) {
                    Rewards = Array.Empty<ITokenReward>();
                } else {
                    Rewards = new ITokenReward[rewardJson.Count];
                    for (var i = 0; i < rewardJson.Count; ++i) {
                        Rewards[i] = new TokenReward((JObject) rewardJson[i]);
                    }
                }
            }
        }
        
        private class PveExplodeResponse : IPveExplodeResponse {
            public HeroId HeroId { get; }
            public int Energy { get; }
            public List<IPveBlockData> DestroyedBlocks { get; }
            public IPveHeroDangerous Dangerous { get; }
            public TrialState IsTrial { get; }
            public List<RewardType> AttendPools { get; }

            public PveExplodeResponse(ISFSObject data) {
                var id = (int) data.GetLong(SFSDefine.SFSField.Id);
                var type = data.ContainsKey(SFSDefine.SFSField.AccountType)
                    ? data.GetInt(SFSDefine.SFSField.AccountType)
                    : data.GetInt(SFSDefine.SFSField.HeroType);
                HeroId = new HeroId(id, (HeroAccountType) type);
                Energy = data.GetInt(SFSDefine.SFSField.Enegy);
                DestroyedBlocks = new List<IPveBlockData>();
                var array = data.GetSFSArray(SFSDefine.SFSField.BLocks);
                foreach (ISFSObject d in array) {
                    DestroyedBlocks.Add(new PveBlockData(d));
                }
                var attendPoolsArray = data.GetIntArray(SFSDefine.SFSField.AttendPools);
                AttendPools = attendPoolsArray.Select(item => (RewardType)item).ToList();

                Dangerous = new PveHeroDangerous(data);
                if (data.ContainsKey("is_trial")) {
                    IsTrial = data.GetBool("is_trial") ? TrialState.TrialBegin : TrialState.TrialEnd;
                }
            }
        }
        
        private class PveBlockData : IPveBlockData {
            public Vector2Int Coord { get; }
            public int Type { get; }
            public int Hp { get; }
            public int MaxHp { get; }
            public List<ITokenReward> Rewards { get; }

            public PveBlockData(ISFSObject data) {
                var i = data.GetInt("i");
                var j = data.GetInt("j");
                Coord = new Vector2Int(i, j);
                Type = data.GetInt("type");
                Hp = data.GetInt("hp");
                MaxHp = data.GetInt("maxHp");
                Rewards = new List<ITokenReward>();
                var rewards = data.GetSFSArray("rewards");
                if (rewards != null) {
                    foreach (ISFSObject r in rewards) {
                        Rewards.Add(new TokenReward(r));
                    }
                }
            }
        }
        
        private class PveHeroDangerous : IPveHeroDangerous {
            public PveDangerousType DangerousType { get; }
            public HeroId HeroId { get; }
            public bool HasNewState { get; }
            public HeroStage State { get; }

            public PveHeroDangerous(ISFSObject data) {
                DangerousType = (PveDangerousType) data.GetInt("is_dangerous");
                var heroId = (int) data.GetLong(SFSDefine.SFSField.Id);
                var heroType = data.ContainsKey(SFSDefine.SFSField.AccountType)
                    ? data.GetInt(SFSDefine.SFSField.AccountType)
                    : data.GetInt(SFSDefine.SFSField.HeroType);
                HeroId = new HeroId(heroId, (HeroAccountType) heroType);
                HasNewState = false;
            }

            public PveHeroDangerous(HeroId heroId, HeroStage state, PveDangerousType type) {
                HeroId = heroId;
                DangerousType = type;
                HasNewState = true;
                State = state;
            }
        }
        
        private class StartPveResponse : IStartPveResponse {
            public List<IPveHeroDangerous> DangerousData { get; }
            public TrialState IsTrial { get; }

            public StartPveResponse(ISFSObject data) {
                DangerousData = new List<IPveHeroDangerous>();
                var arr = data.GetSFSArray("dangerous");
                if (arr != null) {
                    foreach (ISFSObject d in arr) {
                        DangerousData.Add(new PveHeroDangerous(d));
                    }
                }
                if (data.ContainsKey("is_trial")) {
                    IsTrial = data.GetBool("is_trial") ? TrialState.TrialBegin : TrialState.TrialEnd;
                }
            }
        }
        
        private class SyncHeroResponse : ISyncHeroResponse {
            public IHeroDetails[] Details { get; }
            public int[] NewIds { get; }

            public SyncHeroResponse(IHeroDetails[] details) {
                Details = details;
                NewIds = Array.Empty<int>();
            }
        }
    }
}