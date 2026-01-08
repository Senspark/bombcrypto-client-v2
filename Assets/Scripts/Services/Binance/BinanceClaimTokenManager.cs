using System;
using System.Threading.Tasks;

namespace App {
    public class BinanceClaimTokenManager : IClaimTokenManager {
        private readonly IServerManager _serverManager;
        private readonly IChestRewardManager _chestRewardManager;
        private readonly IClaimManager _claimManager;
        private readonly IBlockchainManager _blockchainManager;

        public BinanceClaimTokenManager(IServerManager serverManager, IBlockchainManager blockchainManager,
            IChestRewardManager chestRewardManager, IClaimManager claimManager) {
            _serverManager = serverManager;
            _chestRewardManager = chestRewardManager;
            _claimManager = claimManager;
            _blockchainManager = blockchainManager;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
        
        public async Task<float> ClaimToken(BlockRewardType type, int code) {
            if (type == BlockRewardType.Hero) {
                throw new Exception("Wrong Method");
            }
            var result = await _serverManager.General.ApproveClaim(code);
            return result.ClaimedValue;
        }

        public async Task<ClaimHeroResponse> ClaimHero() {
            var (claimed, message) = await ClaimChestHero();
            var giveAwayHero = await _blockchainManager.GetGiveAwayHero();
            var currentClaimableHero = giveAwayHero + await _blockchainManager.GetClaimableHero();
            var claimableHero = currentClaimableHero;
        
            if (claimed > 0) {
                // Wait for claimable hero changes.
                for (var times = 0; times < 5; ++times) {
                    if (currentClaimableHero != claimableHero) {
                        break;
                    }
                    await WebGLTaskDelay.Instance.Delay(10000);
                    currentClaimableHero = giveAwayHero + await _blockchainManager.GetClaimableHero();
                }
            }
        
            // Claim.
            var processed = false;
            if (currentClaimableHero > 0) {
                if (await _blockchainManager.ClaimHero()) {
                    processed = true;
                }
            }
            if (giveAwayHero > 0) {
                if (await _blockchainManager.ClaimGiveAwayHero()) {
                    processed = true;
                }
            }
        
            // Process token requests.
            var processToken = await _blockchainManager.GetPendingHero();
            if (processToken.pendingHeroes > 0) {
                var detail = await _blockchainManager.ProcessTokenRequests();
                processed = detail.result;
            }
            return new ClaimHeroResponse(claimed, processed, message);
        }
        
        private async Task<(int, string)> ClaimChestHero() {
            var chestHero = _chestRewardManager.GetChestReward(BlockRewardType.Hero);
            if (chestHero <= 0) {
                return (0, null);
            }
            string message = null;
            var claimedHero = 0;
            try {
                var result = await _claimManager.ClaimHero();
                claimedHero += result;
                await _serverManager.General.GetChestReward();
            } catch (Exception ex) {
                message = ex.Message;
            }
            return (claimedHero, message);
        }
    }
}