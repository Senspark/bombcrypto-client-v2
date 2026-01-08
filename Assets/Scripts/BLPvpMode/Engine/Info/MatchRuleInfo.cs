using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchRuleInfo : IMatchRuleInfo {
        [JsonProperty("room_size")]
        public int RoomSize { get; set; }

        [JsonProperty("team_size")]
        public int TeamSize { get; set; }

        [JsonProperty("can_draw")]
        public bool CanDraw { get; set; }

        [JsonProperty("round")]
        public int Round { get; set; }

        [JsonProperty("is_tournament")]
        public bool IsTournament { get; set; }
    }
}