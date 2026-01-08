namespace Data {
    public class HeroColorData {
        public int HeroColor { get; }
        public int HeroId { get; }

        public HeroColorData(int heroColor, int heroId) {
            HeroColor = heroColor;
            HeroId = heroId;
        }
    }
}