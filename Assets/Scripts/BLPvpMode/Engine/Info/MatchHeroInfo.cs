using System.Collections.Generic;

using Newtonsoft.Json;

namespace BLPvpMode.Engine.Info {
    public class MatchHeroInfo : IMatchHeroInfo {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("skin")]
        public int Skin { get; set; }

        [JsonProperty("skin_chests")]
        public Dictionary<int, int[]> SkinChests { get; set; }

        [JsonProperty("health")]
        public int Health { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        [JsonProperty("damage")]
        public int Damage { get; set; }

        [JsonProperty("bomb_count")]
        public int BombCount { get; set; }

        [JsonProperty("bomb_range")]
        public int BombRange { get; set; }

        [JsonProperty("max_health")]
        public int MaxHealth { get; set; }

        [JsonProperty("max_speed")]
        public int MaxSpeed { get; set; }

        [JsonProperty("max_damage")]
        public int MaxDamage { get; set; }

        [JsonProperty("max_bomb_count")]
        public int MaxBombCount { get; set; }

        [JsonProperty("max_bomb_range")]
        public int MaxBombRange { get; set; }
    }
}