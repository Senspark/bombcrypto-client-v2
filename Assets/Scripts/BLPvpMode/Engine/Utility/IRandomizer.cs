namespace BLPvpMode.Engine.Utility {
    public interface IRandomizer<out T> {
        T Random(IRandom random);
    }
}