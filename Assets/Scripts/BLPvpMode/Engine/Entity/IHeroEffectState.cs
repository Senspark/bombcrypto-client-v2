namespace BLPvpMode.Engine.Entity {
    public interface IHeroEffectState {
        bool IsActive { get; }
        HeroEffectReason Reason { get; }
        int Timestamp { get; }
        int Duration { get; }
        long Encode();
    }
}