namespace BLPvpMode.Engine.Info {
    public interface IMatchRuleInfo {
        int RoomSize { get; }
        int TeamSize { get; }
        int Round { get; }
        bool CanDraw { get; }
        bool IsTournament { get; }
    }
}