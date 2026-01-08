namespace App {
    public interface IServerConfig {
        string Host { get; }
        int Port { get; }
        string UdpHost { get; }
        int UdpPort { get; }
        bool IsEncrypted { get; }
        bool UseWebSocket { get; }
        bool IsUseUdp { get; }
        bool Debug { get; }
        string Zone { get; }
        string Room { get; }
        string SaltKey { get; }
        int Version { get; }
    }
}