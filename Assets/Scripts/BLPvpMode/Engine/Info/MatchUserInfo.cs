using System.Collections.Generic;

using App;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchUserInfo : IMatchUserInfo {
        [JsonProperty("server_id")]
        public string ServerId { get; set; }

        [JsonProperty("build_version")]
        public int BuildVersion { get; set; }

        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("mode")]
        public int Mode { get; set; }

        [JsonProperty("is_test")]
        public bool IsTest { get; set; }

        [JsonProperty("is_whitelisted")]
        public bool IsWhitelisted { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("total_match_count")]
        public int TotalMatchCount { get; set; }

        [JsonProperty("match_count")]
        public int MatchCount { get; set; }

        [JsonProperty("win_match_count")]
        public int WinMatchCount { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("point")]
        public int Point { get; set; }

        [JsonProperty("boosters")]
        public int[] Boosters { get; set; }

        [JsonProperty("available_boosters")]
        public Dictionary<int, int> AvailableBoosters { get; set; }

        [JsonProperty("hero")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchHeroInfo>))]
        public IMatchHeroInfo Hero { get; set; }

        [JsonProperty("avatar")]
        public int Avatar { get; set; } = -1;
    }
}