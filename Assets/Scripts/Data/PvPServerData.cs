using Newtonsoft.Json;

namespace Data {
    public class PvPServerData {
        [JsonProperty("id")]
        public string ServerId;

        [JsonProperty("host")]
        public string Host;

        [JsonProperty("port")]
        public int Port;

        [JsonProperty("use_ssl")]
        public bool UseSSL;

        [JsonProperty("udp_host")]
        public string UdpHost;

        [JsonProperty("udp_port")]
        public int UdpPort;
    }
}