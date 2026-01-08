using System;

using App;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    [Flags]
    public enum PvpMode {
        FFA_2 = 1 << 0,
        FFA_3 = 1 << 1,
        FFA_4 = 1 << 2,
        Team_2v2 = 1 << 3,
        FFA_2_B3 = 1 << 4,
        FFA_2_B5 = 1 << 5,
        FFA_2_B7 = 1 << 6,
    }

    public class MatchInfo : IMatchInfo {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("server_id")]
        public string ServerId { get; set; }
        
        [JsonProperty("server_detail")]
        public string ServerDetail { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("mode")]
        public PvpMode Mode { get; set; }

        [JsonProperty("rule")]
        [JsonConverter(typeof(ConcreteTypeConverter<MatchRuleInfo>))]
        public IMatchRuleInfo Rule { get; set; }

        [JsonProperty("team", ItemConverterType = typeof(ConcreteTypeConverter<MatchTeamInfo>))]
        public IMatchTeamInfo[] Team { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("info", ItemConverterType = typeof(ConcreteTypeConverter<MatchUserInfo>))]
        public IMatchUserInfo[] Info { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public static class MatchInfoExtensions {
        public static bool IsParticipant(this IMatchInfo info) {
            return info.Slot < info.Info.Length;
        }

        public static bool IsObserver(this IMatchInfo info) {
            return !info.IsParticipant();
        }
    }
}