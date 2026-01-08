using UnityEngine;

namespace App {
    public class EditorServerConfig : IServerConfig {
        public string Host { get; }
        public int Port { get; }
        public string UdpHost => "";
        public int UdpPort => -1;
        public bool IsEncrypted { get; }
        public bool UseWebSocket { get; }
        public bool IsUseUdp => false;
        public bool Debug { get; }
        public string SaltKey { get; }
        public int Version { get; }
        public string Zone => "BomberGameZone";
        public string Room => "TheLobby";

        public EditorServerConfig(int version, string host, int port, bool enableLog, bool isEncrypted,
            string saltKey) {
            Version = version;
            Host = host;
            Debug = enableLog;
            IsEncrypted = isEncrypted;
            Port = port;
            SaltKey = saltKey;
            UseWebSocket = Application.platform == RuntimePlatform.WebGLPlayer
                           || port == ServerAddress.WssPort
                           || port == ServerAddress.WsPort;
        }
    }
}