namespace Data {
    public class HeroStatData {
        public int HeroId { get; }
        public StatData[] Stats { get; }

        public HeroStatData(int heroId, StatData[] stats) {
            HeroId = heroId;
            Stats = stats;
        }
    }
}