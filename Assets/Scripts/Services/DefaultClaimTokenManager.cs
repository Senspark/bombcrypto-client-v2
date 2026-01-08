using System;
using System.Threading.Tasks;

namespace App {
    public class DefaultClaimTokenManager : IClaimTokenManager {
        private readonly IClaimTokenManager _bridge;

        public DefaultClaimTokenManager(
            IServerManager serverManager,
            IBlockchainManager blockchainManager,
            IChestRewardManager chestRewardManager
        ) {
            // Sử dụng hoàn toàn như polygon
            _bridge = new PolygonClaimTokenManager(serverManager, blockchainManager, chestRewardManager);
        }
        
        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }
        
        public void Destroy() {
            _bridge.Destroy();
        }

        public Task<float> ClaimToken(BlockRewardType type, int code) {
            return _bridge.ClaimToken(type, code);
        }

        public Task<ClaimHeroResponse> ClaimHero() {
            return _bridge.ClaimHero();
        }
    }
}