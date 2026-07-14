using System.Threading.Tasks;

namespace App {
    public class RoninNetworkConfig : INetworkConfig {
        public NetworkType NetworkType => NetworkType.Ronin;
        public string Domain => "https://app.bombcrypto.io/";
        public string NetworkName => "ronin";

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}
