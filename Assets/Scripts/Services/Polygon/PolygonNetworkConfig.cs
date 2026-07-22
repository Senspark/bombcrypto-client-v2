using System.Threading.Tasks;

namespace App {
    public class PolygonNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Polygon;
        public string Domain => "https://app.bombcrypto.io/polygon/";
        public string NetworkName => "polygon";

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}
