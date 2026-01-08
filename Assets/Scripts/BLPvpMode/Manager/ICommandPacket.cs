namespace BLPvpMode.Manager {
    public interface ICommandPacket {
        /// <summary>
        /// Gets the packet timestamp.
        /// </summary>
        long Timestamp { get; }
    }
}