namespace Data {
    public class ProductHeroData {
        public ProductData DataBase { get; }
        public AbilityData[] Abilities { get; }
        public int HeroColor { get; }
        public int HeroId { get; }
        public StatData[] Stats { get; }

        public ProductHeroData(
            ProductData dataBase,
            AbilityData[] abilities,
            int heroColor,
            int heroId,
            StatData[] stats
        ) {
            DataBase = dataBase;
            Abilities = abilities;
            HeroColor = heroColor;
            HeroId = heroId;
            Stats = stats;
        }
    }
}