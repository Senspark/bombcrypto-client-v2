namespace BLPvpMode.Engine.Utility {
    public interface IRandom {
        int RandomInt(int minInclusive, int maxExclusive);
        float RandomFloat(float minInclusive, float maxExclusive);
    }
}