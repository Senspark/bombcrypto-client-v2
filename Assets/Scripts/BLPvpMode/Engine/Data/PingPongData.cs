using System;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class PingPongData : IPingPongData {
        public int RequestId { get; }
        public int[] Latencies { get; }
        public int[] TimeDelta { get; }
        public float[] LossRates { get; }

        public static IPingPongData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<PingPongData>(json);
            return result;
        }

        public static IPingPongData FastParse(ISFSObject data) {
            var requestId = data.GetInt("request_id");
            var latenciesArray = data.GetSFSArray("latencies");
            var latencies = new int[latenciesArray.Count];
            for (var i = 0; i < latenciesArray.Count; ++i) {
                latencies[i] = latenciesArray.GetInt(i);
            }
            var timeDeltaArray = data.GetSFSArray("time_delta");
            var timeDelta = new int[timeDeltaArray.Count];
            for (var i = 0; i < timeDeltaArray.Count; ++i) {
                timeDelta[i] = timeDeltaArray.GetInt(i);
            }
            var lossRatesArray = data.GetSFSArray("loss_rates");
            var lossRates = new float[lossRatesArray.Count];
            for (var i = 0; i < lossRatesArray.Count; ++i) {
                lossRates[i] = (float)lossRatesArray.GetDouble(i);
            }
            return new PingPongData(requestId, latencies, timeDelta, lossRates);
        }

        public PingPongData(
            [JsonProperty("request_id")] int requestId,
            [JsonProperty("latencies")] int[] latencies,
            [JsonProperty("time_delta")] int[] timeDelta,
            [JsonProperty("loss_rates")] float[] lossRates
        ) {
            RequestId = requestId;
            Latencies = latencies;
            TimeDelta = timeDelta;
            LossRates = lossRates;
        }
    }
}