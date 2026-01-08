using System.Collections.Generic;

using App;

using BLPvpMode.Engine.Info;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Data {
    public class MatchData : IMatchData {
        public string Id { get; }
        public MatchStatus Status { get; set; }
        public int ObserverCount { get; }
        public long StartTimestamp { get; }
        public long ReadyStartTimestamp { get; set; }
        public long RoundStartTimestamp { get; set; }
        public int Round { get; set; }
        public List<IMatchResultInfo> Results { get; }

        public MatchData(
            [JsonProperty("id")] string id,
            [JsonProperty("status")] int status,
            [JsonProperty("observer_count")] int observerCount,
            [JsonProperty("start_timestamp")] long startTimestamp,
            [JsonProperty("ready_start_timestamp")]
            long readyStartTimestamp,
            [JsonProperty("round_start_timestamp")]
            long roundStartTimestamp,
            [JsonProperty("round")] int round,
            [JsonProperty("results", ItemConverterType = typeof(ConcreteTypeConverter<MatchResultInfo>))]
            List<IMatchResultInfo> results) {
            Id = id;
            Status = (MatchStatus) status;
            ObserverCount = observerCount;
            StartTimestamp = startTimestamp;
            ReadyStartTimestamp = readyStartTimestamp;
            RoundStartTimestamp = roundStartTimestamp;
            Round = round;
            Results = results;
        }
    }
}