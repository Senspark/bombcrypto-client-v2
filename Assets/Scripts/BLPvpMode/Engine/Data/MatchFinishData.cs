using App;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class MatchFinishData : IMatchFinishData {
        [JsonProperty("match")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchData>))]
        public IMatchData MatchData { get; set; }

        public static IMatchFinishData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<MatchFinishData>(json);
            return result;
        }
    }
}