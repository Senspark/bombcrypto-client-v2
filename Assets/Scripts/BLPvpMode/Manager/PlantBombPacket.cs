namespace BLPvpMode.Manager {
    public interface IPlantBombPacket : ICommandPacket { }

    public class PlantBombPacket : IPlantBombPacket {
        public long Timestamp { get; set; }
    }
}