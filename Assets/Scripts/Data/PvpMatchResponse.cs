using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Newtonsoft.Json;

namespace Data {
    public class PvpMatchResponse {
        [JsonProperty("info")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchInfo>))]
        public IMatchInfo Info;

        [JsonProperty("hash")]
        public string Hash;

        public static PvpMatchResponse Parse(string data) {
            var result = JsonConvert.DeserializeObject<PvpMatchResponse>(data);
            // FIXME: hash may be null (sent from server).
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (result.Info.Hash == null) {
                // ReSharper disable once HeuristicUnreachableCode
                ((MatchInfo) result.Info).Hash = result.Hash;
            }
            return result;
        }
    }
}