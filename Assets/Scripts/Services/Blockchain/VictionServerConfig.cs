using System.Threading.Tasks;

namespace App {
    public class VictionNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Viction;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "viction";
        public IBlockchainConfig BlockchainConfig { get; }
        public VictionNetworkConfig(bool production) {
            BlockchainConfig = new VictionBlockchainConfig(production);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}