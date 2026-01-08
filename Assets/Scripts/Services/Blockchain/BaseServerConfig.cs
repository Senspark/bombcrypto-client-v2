using System.Threading.Tasks;

namespace App {
    public class BaseNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Base;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "base";
        public IBlockchainConfig BlockchainConfig { get; }
        public BaseNetworkConfig(bool production) {
            BlockchainConfig = new BaseBlockchainConfig(production);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}