namespace Data {
    public class InventoryHeroData {
        public AbilityData[] Abilities { get; }
        public int Color { get; }
        public int HeroId { get; }
        public string HeroName { get; }
        public int HeroType { get; }
        public int Quantity { get; }
        public bool Sellable { get; }
        public HeroSkinData[] Skins { get; }
        public StatData[] Stats { get; }
        public bool IsNew { get;}

        public InventoryHeroData(
            AbilityData[] abilities,
            int color,
            int heroId,
            string heroName,
            int heroType,
            int quantity,
            bool sellable,
            HeroSkinData[] skins,
            StatData[] stats,
            bool isNew
        ) {
            Abilities = abilities;
            Color = color;
            HeroId = heroId;
            HeroName = heroName;
            HeroType = heroType;
            Quantity = quantity;
            Sellable = sellable;
            Skins = skins;
            Stats = stats;
            IsNew = isNew;
        }
    }
}