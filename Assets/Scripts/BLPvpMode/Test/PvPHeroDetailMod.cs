using Newtonsoft.Json;

namespace BLPvpMode.Test {
    public class PvPHeroDetailMod {
        [JsonProperty("skin")]
        public int Skin { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("bomb_skin")]
        public int BombSkin { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        public int MaxSpeed { get; }

        [JsonProperty("bomb_range")]
        public int BombRange { get; set; }

        public int MaxRange { get; }

        [JsonProperty("bomb_count")]
        public int BombCount { get; set; }

        public int MaxBomb { get; }

        [JsonProperty("hp")]
        public int Health { get; set; }
        
        [JsonProperty("dmg")]
        public int Damage { get; set; }
        
        [JsonProperty("pos_x")]
        public float PosX { get; set; }

        [JsonProperty("pos_y")]
        public float PosY { get; set; }

        [JsonProperty("last_sync_position_timestamp")]
        public long LastSyncPositionTimestamp { get; set; }

        [JsonProperty("skin_chests")]
        public int[][] SkinChests { get; set; }
    }
}