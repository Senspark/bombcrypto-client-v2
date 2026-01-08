using System.Diagnostics.CodeAnalysis;

using Constant;

namespace Data {
    public class FreeTRHeroData {
        public AbilityData[] Abilities { get; }
        public int HeroColor { get; }
        public int HeroId { get; }
        [NotNull] public string HeroName { get; }
        public int HeroType { get; }
        public int ItemId { get; }
        public StatData[] Stats { get; }

        public FreeTRHeroData(
            AbilityData[] abilities,
            int heroColor,
            int heroId,
            [NotNull] string heroName,
            HeroType heroType,
            int itemId,
            StatData[] stats
        ) : this(abilities, heroColor, heroId, heroName, (int) heroType, itemId, stats) {
        }

        public FreeTRHeroData(
            AbilityData[] abilities,
            int heroColor,
            int heroId,
            string heroName,
            int heroType,
            int itemId,
            StatData[] stats
        ) {
            Abilities = abilities;
            HeroColor = heroColor;
            HeroId = heroId;
            HeroName = heroName;
            HeroType = heroType;
            ItemId = itemId;
            Stats = stats;
        }
    }
}