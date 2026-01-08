using Newtonsoft.Json;

namespace Data {
    public class EarlyConfigItemData {
        [JsonProperty("abilities")]
        public string Abilities;
        
        [JsonProperty("kind")]
        public string Kind;
        
        [JsonProperty("description_en")]
        public string Description;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("item_id")]
        public int ItemId;

        [JsonProperty("type")]
        public int ItemType;

        [JsonProperty("tag")]
        public int TagShop;
    }
}