using System.Collections.Generic;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class PvpResultUserInfo : IPvpResultUserInfo {
        public string ServerId { get; }
        public bool IsBot { get; }
        public int TeamId { get; }
        public int UserId { get; }
        public string Username { get; }
        public int Rank { get; }
        public int Point { get; }
        public int MatchCount { get; }
        public int WinMatchCount { get; }
        public int DeltaPoint { get; }
        public Dictionary<int, int> UsedBoosters { get; }
        public bool Quit { get; }
        public Dictionary<int, float> Rewards { get; }

        public PvpResultUserInfo(
            [JsonProperty("server_id")] string serverId,
            [JsonProperty("is_bot")] bool isBot,
            [JsonProperty("team_id")] int teamId,
            [JsonProperty("user_id")] int userId,
            [JsonProperty("username")] string username,
            [JsonProperty("rank")] int rank,
            [JsonProperty("point")] int point,
            [JsonProperty("match_count")] int matchCount,
            [JsonProperty("win_match_count")] int winMatchCount,
            [JsonProperty("delta_point")] int deltaPoint,
            [JsonProperty("used_boosters")] Dictionary<int, int> usedBoosters,
            [JsonProperty("quit")] bool quit,
            [JsonProperty("rewards")] Dictionary<int, float> rewards) {
            ServerId = serverId;
            IsBot = isBot;
            TeamId = teamId;
            UserId = userId;
            Username = username;
            Rank = rank;
            Point = point;
            MatchCount = matchCount;
            WinMatchCount = winMatchCount;
            DeltaPoint = deltaPoint;
            UsedBoosters = usedBoosters;
            Quit = quit;
            Rewards = rewards;
        }
    }
}