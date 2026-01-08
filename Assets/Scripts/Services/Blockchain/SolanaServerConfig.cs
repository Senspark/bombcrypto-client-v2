using System.Threading.Tasks;

namespace App {
    public class SolanaNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Solana;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "solana";
        public IBlockchainConfig BlockchainConfig { get; }
        public SolanaNetworkConfig(bool production) {
            BlockchainConfig = new SolanaBlockchainConfig(production);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}