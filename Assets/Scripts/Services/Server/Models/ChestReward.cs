using System.Collections.Generic;

using App;

using Newtonsoft.Json.Linq;

using Scenes.TreasureModeScene.Scripts.Solana.Server_Response;

using Sfs2X.Entities.Data;

namespace Server.Models {
    public class ChestReward : IChestReward {
        public List<ITokenReward> Rewards { get; }

        public ChestReward(List<ITokenReward> rewards) {
            Rewards = rewards;
        }
    }
    
    public class TokenReward : ITokenReward {
        public IRewardType Type { get; }
        public string Network { get; }
        public float Value { get; }
        public float ClaimPending { get; }

        public TokenReward(JObject data) {
            var type = (string) data["type"];
            Type = new RewardType(type);
            Value = (float) data["value"];
            ClaimPending = (float) data["claimPending"];
            Network = data["data_type"].Value<string>();
        }

        public TokenReward(ISFSObject data) {
            var type = data.GetUtfString("type");
            Type = new RewardType(type);
            Value = data.GetFloat("value");
            ClaimPending = (float) data.GetDouble("claimPending");
            Network = data.GetUtfString("data_type");
        }

        public TokenReward(string tokenName, string network) {
            Type = new RewardType(tokenName);
            Network = network;
        }
    }
    
    public class RewardType : IRewardType {
        public BlockRewardType Type { get; }
        public string Name { get; }

        public RewardType(string name) {
            Name = name;
            Type = RewardUtils.ConvertToBlockRewardType(name);
        }

        public RewardType(BlockRewardType type) {
            Type = type;
            Name = RewardUtils.ConvertToBlockRewardType(type);
        }
    }
}