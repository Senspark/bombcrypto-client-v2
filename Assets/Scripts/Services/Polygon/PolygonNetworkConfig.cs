using System.Threading.Tasks;

namespace App {
    public class PolygonNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Polygon;
        public string Domain => "https://app.bombcrypto.io/polygon/";
        public string NetworkName => "polygon";
        public IBlockchainConfig BlockchainConfig { get; }
        public PolygonNetworkConfig(bool production) {
            BlockchainConfig = new PolygonBlockchainConfig(production);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}