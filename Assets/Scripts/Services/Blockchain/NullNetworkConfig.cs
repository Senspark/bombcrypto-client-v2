using System.Threading.Tasks;

namespace App {
    public class NullNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Binance;
        public string Domain => string.Empty;
        public string NetworkName => "binance";
        public IBlockchainConfig BlockchainConfig => null;
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}