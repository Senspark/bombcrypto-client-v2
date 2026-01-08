namespace Data {
    public class HeroAbilityData {
        public AbilityData[] Abilities { get; }
        public int HeroId { get; }

        public HeroAbilityData(AbilityData[] abilities, int heroId) {
            Abilities = abilities;
            HeroId = heroId;
        }
    }
}