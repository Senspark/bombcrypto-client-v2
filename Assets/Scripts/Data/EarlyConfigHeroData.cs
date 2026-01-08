using Newtonsoft.Json;

namespace Data {
    public class EarlyConfigHeroData {
        [JsonProperty("color")]
        public int Color;

        [JsonProperty("skin")]
        public int Skin;

        [JsonProperty("hp")]
        public int Health;
        
        [JsonProperty("bomb")]
        public int Bomb;

        [JsonProperty("speed")]
        public int Speed;

        [JsonProperty("tutorial")]
        public int Tutorial;

        [JsonProperty("maxHp")]
        public int MaxHealth;

        [JsonProperty("item_id")]
        public int ItemId;

        [JsonProperty("maxRange")]
        public int MaxRange;

        [JsonProperty("maxSpeed")]
        public int MaxSpeed;

        [JsonProperty("maxBomb")]
        public int MaxBomb;

        [JsonProperty("bomb_range")]
        public int BombRange;

        [JsonProperty("dmg")]
        public int Damage;

        [JsonProperty("maxDmg")]
        public int MaxDamage;

        [JsonProperty("maxUpgradeSpeed")]
        public int MaxUpgradeSpeed;

        [JsonProperty("maxUpgradeRange")]
        public int MaxUpgradeRange;

        [JsonProperty("maxUpgradeBomb")]
        public int MaxUpgradeBomb;

        [JsonProperty("maxUpgradeHp")]
        public int MaxUpgradeHealth;

        [JsonProperty("maxUpgradeDmg")]
        public int MaxUpgradeDamage;
    }
}