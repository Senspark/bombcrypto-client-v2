using Newtonsoft.Json;

namespace Data {
    public class ConfigUpgradeData {
        [JsonProperty("grind_config")]
        public ConfigGrindData[] Grinds;

        [JsonProperty("upgrade_hero_config")]
        public ConfigUpgradeHeroData[] Heroes;

        [JsonProperty("upgrade_crystal_config")]
        public ConfigUpgradeCrystalData[] Crystals;
    }
}