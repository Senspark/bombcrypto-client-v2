namespace Data {
    public class PvPServerConfigData {
        public PvPServerData[] Servers;
        public ZoneData[] Zones;

        public static implicit operator (PvPServerData[] Servers, ZoneData[] Zones)(PvPServerConfigData data) {
            return (data.Servers, data.Zones);
        }
    }
}