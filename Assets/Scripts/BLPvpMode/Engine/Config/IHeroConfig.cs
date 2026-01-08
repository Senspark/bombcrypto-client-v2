namespace BLPvpMode.Engine.Config {
    public interface IHeroConfig {
        int ExplodeDuration { get; }
        int ShieldedDuration { get; }
        int InvincibleDuration { get; }
        int ImprisonedDuration { get; }
        int SkullEffectDuration { get; }
    }
}