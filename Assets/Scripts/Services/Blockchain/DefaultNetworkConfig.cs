using System;
using System.Threading.Tasks;

namespace App {
    public class DefaultNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => _bridge.NetworkType;
        public string Domain => _bridge.Domain;
        public string NetworkName => _bridge.NetworkName;

        private readonly INetworkConfig _bridge;

        public DefaultNetworkConfig(NetworkType chainName) {
            _bridge = chainName switch {
                //DevHoang: Add new airdrop
                NetworkType.Binance => new BinanceNetworkConfig(),
                NetworkType.Polygon => new PolygonNetworkConfig(),
                NetworkType.Ton => new TonNetworkConfig(),
                NetworkType.Solana => new SolanaNetworkConfig(),
                NetworkType.Ronin => new RoninNetworkConfig(),
                NetworkType.Base => new BaseNetworkConfig(),
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
