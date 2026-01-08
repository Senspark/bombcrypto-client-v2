using Newtonsoft.Json;

namespace Data {
    public class EarlyConfigRankData {
        [JsonProperty("bomb_rank")]
        public int BombRank;

        [JsonProperty("start_point")]
        public int StartPoint;

        [JsonProperty("end_point")]
        public int EndPoint;

        [JsonProperty("name")]
        public string Name;
    }
}