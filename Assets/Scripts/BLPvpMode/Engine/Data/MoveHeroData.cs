using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace BLPvpMode.Engine.Data {
    public class MoveHeroData : IMoveHeroData {
        public Vector2 Position { get; }

        public static IMoveHeroData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<MoveHeroData>(json);
            return result;
        }

        public static IMoveHeroData FastParse(ISFSObject data) {
            var x = data.GetFloat("x");
            var y = data.GetFloat("y");
            return new MoveHeroData(x, y);
        }

        public MoveHeroData(
            [JsonProperty("x")] float x,
            [JsonProperty("y")] float y
        ) {
            Position = new Vector2(x, y);
        }
    }
}