namespace App {
    public class BinanceServerConfig : IServerConfig {
        public string Host => "server.bombcrypto.io";
        public int Port => 8443;
        public string UdpHost => "";
        public int UdpPort => -1;
        public bool IsEncrypted => true;
        public bool UseWebSocket => true;
        public bool IsUseUdp => false;
        public bool Debug => false;
        public string Zone => "BomberGameZone";
        public string Room => "TheLobby";
        public string SaltKey { get; }
        public int Version { get; }

        public BinanceServerConfig(int version, string salt) {
            Version = version;
            SaltKey = salt;
        }
    }
}