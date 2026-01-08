using System.Threading.Tasks;

namespace App {
    public class TonNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Ton;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "binance";
        public IBlockchainConfig BlockchainConfig { get; }
        public TonNetworkConfig(bool production) {
            BlockchainConfig = new TonBlockchainConfig(production);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}