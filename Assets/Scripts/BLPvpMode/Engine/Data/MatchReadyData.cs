using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class MatchReadyData : IMatchReadyData {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        public static IMatchReadyData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<MatchReadyData>(json);
            return result;
        }
    }
}