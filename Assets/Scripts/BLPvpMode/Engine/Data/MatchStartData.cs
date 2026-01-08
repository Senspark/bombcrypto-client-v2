using App;

using BLPvpMode.Engine.Info;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class MatchStartData : IMatchStartData {
        public IMatchData Match { get; }
        public IMapInfo Map { get; }

        public static IMatchStartData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<MatchStartData>(json);
            return result;
        }

        public MatchStartData(
            [JsonProperty("match")] [JsonConverter(typeof(ConcreteTypeConverter<MatchData>))]
            IMatchData match,
            [JsonProperty("map")] [JsonConverter(typeof(ConcreteTypeConverter<MapInfo>))]
            IMapInfo map) {
            Match = match;
            Map = map;
        }
    }
}