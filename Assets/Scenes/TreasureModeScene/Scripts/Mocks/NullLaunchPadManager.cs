using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Services.Rewards;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullLaunchPadManager : ILaunchPadManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public Task SyncRemoteData() {
            return Task.CompletedTask;
        }

        public bool CanShowInLaunchPad(IRewardType type, NetworkSymbol symbol) {
            return false;
        }

        public bool CanShowInLaunchPad(ITokenReward type) {
            return false;
        }

        public bool CanClaim(IRewardType type, NetworkSymbol symbol, float rewardValue) {
            return false;
        }

        public (float, string) GetClaimFee(IRewardType type, NetworkSymbol symbol) {
            throw new System.NotImplementedException();
        }

        public TokenData GetData(IRewardType type, NetworkSymbol symbol) {
            throw new System.NotImplementedException();
        }

        public TokenData GetData(ITokenReward type) {
            throw new System.NotImplementedException();
        }

        public List<TokenData> GetForceDisplayTokens() {
            throw new System.NotImplementedException();
        }

        public List<TokenData> GetForceDisplayTokensTelegram() {
            throw new System.NotImplementedException();
        }

        public List<TokenData> GetForceDisplayTokensSolana() {
            throw new System.NotImplementedException();
        }
        
        public List<TokenData> GetForceDisplayTokensRonin() {
            throw new System.NotImplementedException();
        }
        
        public List<TokenData> GetForceDisplayTokensBase() {
            throw new System.NotImplementedException();
        }
        
        public List<TokenData> GetForceDisplayTokensViction() {
            throw new System.NotImplementedException();
        }

        public IRewardType CreateRewardType(string tokenType) {
            throw new System.NotImplementedException();
        }
    }
}