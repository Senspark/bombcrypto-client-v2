using System.Threading.Tasks;

namespace App {
    public class BinanceNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Binance;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "binance";
        public IBlockchainConfig BlockchainConfig { get; }
        public BinanceNetworkConfig(bool production) {
            BlockchainConfig = new BinanceBlockchainConfig(production);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}