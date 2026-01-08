using System;
using System.Threading.Tasks;

namespace App {
    public class DefaultNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => _bridge.NetworkType;
        public string Domain => _bridge.Domain;
        public string NetworkName => _bridge.NetworkName;
        public IBlockchainConfig BlockchainConfig => _bridge.BlockchainConfig;

        private readonly INetworkConfig _bridge;

        public DefaultNetworkConfig(bool production, NetworkType chainName) {
            _bridge = chainName switch {
                //DevHoang: Add new airdrop
                NetworkType.Binance => new BinanceNetworkConfig(production),
                NetworkType.Polygon => new PolygonNetworkConfig(production),
                NetworkType.Ton => new TonNetworkConfig(production),
                NetworkType.Solana => new SolanaNetworkConfig(production),
                NetworkType.Ronin => new RoninNetworkConfig(production),
                NetworkType.Base => new BaseNetworkConfig(production),
                NetworkType.Viction => new VictionNetworkConfig(production),
                _ => throw new ArgumentOutOfRangeException(nameof(chainName), chainName, null)
            };
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}