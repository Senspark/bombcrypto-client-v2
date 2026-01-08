using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchTeamInfo : IMatchTeamInfo {
        [JsonProperty("slots")]
        public int[] Slots { get; set; }
    }
}