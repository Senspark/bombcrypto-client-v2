using Data;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public interface IMatchInfo {
        [NotNull]
        string Id { get; }

        [NotNull]
        string ServerId { get; }
        
        string ServerDetail { get; }

        long Timestamp { get; }
        PvpMode Mode { get; }

        [NotNull]
        IMatchRuleInfo Rule { get; }

        [NotNull]
        IMatchTeamInfo[] Team { get; }

        int Slot { get; }

        [NotNull]
        IMatchUserInfo[] Info { get; }

        [NotNull]
        string Hash { get; }
    }
    [System.Serializable]
    public class Address {
        [JsonProperty("address")]
        public string AddressValue { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }

    [System.Serializable]
    public class Detail {
        [JsonProperty("web")]
        public Address Web { get; set; }

        [JsonProperty("editor")]
        public Address Editor { get; set; }
    }

    [System.Serializable]
    public class ServerDetail {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("useSSl")]
        public bool UseSSl { get; set; }

        [JsonProperty("detail")]
        public Detail Detail { get; set; }
        
        public PvPServerData ConvertToPvpServerData() {
            return new PvPServerData {
                ServerId = Id,
                Host = Detail.Web.AddressValue,
                Port = Detail.Web.Port,
                UseSSL = UseSSl,
                UdpHost = Detail.Editor.AddressValue,
                UdpPort = Detail.Editor.Port
            };
        }
    }

}