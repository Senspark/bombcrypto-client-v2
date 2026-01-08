namespace BLPvpMode.Engine.Command {
    public interface ICommand {
        int Timestamp { get; }

        void Handle();
    }
}