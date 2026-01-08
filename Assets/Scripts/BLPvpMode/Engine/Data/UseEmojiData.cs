using Newtonsoft.Json;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Data {
    public class UseEmojiData : IUseEmojiData {
        public string MatchId { get; }
        public int Slot { get; }
        public int ItemId { get; }

        public static IUseEmojiData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<UseEmojiData>(json);
            return result;
        }

        public static IUseEmojiData FastParse(ISFSObject data) {
            var matchId = data.GetUtfString("match_id");
            var slot = data.GetInt("slot");
            var itemId = data.GetInt("item_id");
            return new UseEmojiData(matchId, slot, itemId);
        }

        public UseEmojiData(
            [JsonProperty("match_id")] string matchId,
            [JsonProperty("slot")] int slot,
            [JsonProperty("item_id")] int itemId
        ) {
            MatchId = matchId;
            Slot = slot;
            ItemId = itemId;
        }
    }
}