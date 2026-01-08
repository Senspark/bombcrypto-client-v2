using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data {
    public class GachaChestNameData {
        public string ChestName { get; }
        public int ChestType { get; }

        [JsonConstructor]
        public GachaChestNameData(
            [JsonProperty("chest_name")] string chestName,
            [JsonProperty("chest_type")] int chestType
        ) {
            ChestName = chestName;
            ChestType = chestType;
        }
        
        public GachaChestNameData(JToken data) {
            ChestName = data["chest_name"].ToObject<string>();
            ChestType = data["chest_type"].ToObject<int>();
        }
    }
}