using Senspark;

namespace App {
    public enum NetworkType {
        //DevHoang: Add new airdrop
        Binance, Polygon, Ton, Solana, Ronin, Base, Viction
    }

    public enum GameVariant {
        OldBomb, BomberLand
    }

    [Service(nameof(INetworkConfig))]
    public interface INetworkConfig : IService {
        NetworkType NetworkType { get; }
        // GameVariant GameVariant { get; }
        string Domain { get; }
        string NetworkName { get; }
        IBlockchainConfig BlockchainConfig { get; }
    }
}