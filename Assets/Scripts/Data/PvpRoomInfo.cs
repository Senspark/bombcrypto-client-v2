using App;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;

using Newtonsoft.Json;

namespace Data {
    public interface IPvpRoomInfo {
        IMatchData MatchData { get; }
        IMatchInfo MatchInfo { get; }
    }

    public class PvpRoomInfo : IPvpRoomInfo {
        [JsonProperty("match_data")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchData>))]
        public IMatchData MatchData { get; set; }

        [JsonProperty("match_info")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchInfo>))]
        public IMatchInfo MatchInfo { get; set; }
    }
}