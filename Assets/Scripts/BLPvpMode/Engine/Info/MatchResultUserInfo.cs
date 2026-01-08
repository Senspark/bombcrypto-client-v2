using System.Collections.Generic;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchResultUserInfo : IMatchResultUserInfo {
        [JsonProperty("server_id")]
        public string ServerId { get; set; }

        [JsonProperty("is_test")]
        public bool IsTest { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("team_id")]
        public int TeamId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("match_count")]
        public int MatchCount { get; set; }

        [JsonProperty("win_match_count")]
        public int WinMatchCount { get; set; }

        [JsonProperty("boosters")]
        public int[] Boosters { get; set; }

        [JsonProperty("used_boosters")]
        public Dictionary<int, int> UsedBoosters { get; set; }

        [JsonProperty("quit")]
        public bool Quit { get; set; }
    }
}