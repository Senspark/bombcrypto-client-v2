using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace BLPvpMode.Engine.Data {
    public class PlantBombData : IPlantBombData {
        public static IPlantBombData Parse(ISFSObject data) {
            var json = data.ToJson();
            var result = JsonConvert.DeserializeObject<PlantBombData>(json);
            return result;
        }

        public static IPlantBombData FastParse(ISFSObject data) {
            var id = data.GetInt("id");
            var x = data.GetInt("x");
            var y = data.GetInt("y");
            var plantTimestamp = data.GetInt("plant_timestamp");
            return new PlantBombData(id, x, y, plantTimestamp);
        }

        public int Id { get; }
        public Vector2Int Position { get; }
        public int PlantTimestamp { get; }

        public PlantBombData(
            [JsonProperty("id")] int id,
            [JsonProperty("x")] int x,
            [JsonProperty("y")] int y,
            [JsonProperty("plant_timestamp")] int plantTimestamp) {
            Id = id;
            Position = new Vector2Int(x, y);
            PlantTimestamp = plantTimestamp;
        }
    }
}