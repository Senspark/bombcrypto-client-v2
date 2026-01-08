using App;

using BLPvpMode.Engine.Info;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class FallingBlockData : IFallingBlockData {
        public string MatchId { get; }
        public IFallingBlockInfo[] Blocks { get; }

        public static IFallingBlockData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<FallingBlockData>(json);
            return result;
        }

        public static IFallingBlockData FastParse(ISFSObject data) {
            var matchId = data.GetUtfString("match_id");
            var blocksArray = data.GetSFSArray("blocks");
            var blocks = new IFallingBlockInfo[blocksArray.Count];
            for (var i = 0; i < blocksArray.Count; ++i) {
                var item = blocksArray.GetSFSObject(i);
                var timestamp = item.GetInt("timestamp");
                var x = item.GetInt("x");
                var y = item.GetInt("y");
                blocks[i] = new FallingBlockInfo(timestamp, x, y);
            }
            return new FallingBlockData(matchId, blocks);
        }

        public FallingBlockData(
            [JsonProperty("match_id")] string matchId,
            [JsonProperty("blocks", ItemConverterType = typeof(ConcreteTypeConverter<FallingBlockInfo>))]
            IFallingBlockInfo[] blocks) {
            MatchId = matchId;
            Blocks = blocks;
        }
    }
}