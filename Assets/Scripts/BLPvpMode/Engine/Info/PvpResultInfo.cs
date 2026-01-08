using App;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Info {
    public class PvpResultInfo : IPvpResultInfo {
        public string Id { get; }
        public PvpMode Mode { get; }
        public bool IsDraw { get; }
        public int WinningTeam { get; }
        public int[] Scores { get; }
        public IPvpResultUserInfo[] Info { get; }

        public static IPvpResultInfo Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<PvpResultInfo>(json);
            return result;
        }

        public PvpResultInfo(
            [JsonProperty("id")] string id,
            [JsonProperty("mode")] int mode,
            [JsonProperty("is_draw")] bool isDraw,
            [JsonProperty("winning_team")] int winningTeam,
            [JsonProperty("scores")] int[] scores,
            [JsonProperty("info", ItemConverterType = typeof(ConcreteTypeConverter<PvpResultUserInfo>))]
            IPvpResultUserInfo[] info) {
            Id = id;
            Mode = (PvpMode) mode;
            IsDraw = isDraw;
            WinningTeam = winningTeam;
            Scores = scores;
            Info = info;
        }
    }
}