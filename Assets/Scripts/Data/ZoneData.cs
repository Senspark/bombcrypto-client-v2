using Newtonsoft.Json;

namespace Data {
    public class ZoneData {
        [JsonProperty("host")]
        public string Host;

        [JsonProperty("id")]
        public string ZoneId;
    }
}