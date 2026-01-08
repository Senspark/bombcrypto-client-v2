using App;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchResultInfo : IMatchResultInfo {
        [JsonProperty("is_draw")]
        public bool IsDraw { get; set; }

        [JsonProperty("winning_team")]
        public int WinningTeam { get; set; }

        [JsonProperty("scores")]
        public int[] Scores { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("start_timestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("info", ItemConverterType = typeof(ConcreteTypeConverter<MatchResultUserInfo>))]
        public IMatchResultUserInfo[] Info { get; set; }
    }
}