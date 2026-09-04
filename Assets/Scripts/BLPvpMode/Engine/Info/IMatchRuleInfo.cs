namespace BLPvpMode.Engine.Info {
    public interface IMatchRuleInfo {
        int RoomSize { get; }
        int TeamSize { get; }
        int Round { get; }
        bool CanDraw { get; }
        bool IsTournament { get; }
        int GameMode { get; }
        int WagerMode { get; }
        int WagerTier { get; }
        int WagerToken { get; }
    }
}