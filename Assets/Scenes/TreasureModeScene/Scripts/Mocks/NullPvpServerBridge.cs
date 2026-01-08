using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PvpMode.Services;
using Services;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullPvpServerBridge : IPvpServerBridge {
        public Task<ISyncPvPConfigResult> SyncPvPConfig() {
            throw new System.NotImplementedException();
        }

        public Task<ISyncPvPHeroesResult> SyncPvPHeroes() {
            throw new System.NotImplementedException();
        }

        public Task JoinQueue(int mode, string matchId, int heroId, int[] boosters, IPingInfo[] pingInfo, int avatarId,
            bool test = false) {
            throw new System.NotImplementedException();
        }

        public Task LeaveQueue() {
            throw new System.NotImplementedException();
        }

        public Task<IBoosterResult> GetUserBooster() {
            throw new System.NotImplementedException();
        }

        public Task BuyBooster(int boosterType) {
            throw new System.NotImplementedException();
        }

        public void ClearCachePvpRanking() {
            throw new System.NotImplementedException();
        }

        public Task<IPvpRankingResult> GetPvpRanking(int page = 1, int size = 100) {
            throw new System.NotImplementedException();
        }

        public Task<IPvpOtherUserInfo> GetOtherUserInfo(int userId, string userName) {
            throw new System.NotImplementedException();
        }

        public Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfig() {
            throw new NotImplementedException();
        }

        public Task<ICoinRankingResult> GetCoinRanking() {
            return Task.FromResult<ICoinRankingResult>(new CoinRankingResult());
        }

        public Task<ICoinRankingResult> GetAllSeasonCoinRanking() {
            throw new System.NotImplementedException();
        }

        public Task<IPvpClaimRewardResult> ClaimPvpReward() {
            throw new System.NotImplementedException();
        }

        public Task<IPvpClaimMatchRewardResult> ClaimMatchReward() {
            throw new System.NotImplementedException();
        }

        public Task<IPvpHistoryResult> GetPvpHistory(int at = 0, int size = 50) {
            throw new System.NotImplementedException();
        }

        public Task<IOpenChestResult> OpenChest() {
            throw new System.NotImplementedException();
        }

        public Task<IGetEquipmentResult> GetEquipment() {
            throw new System.NotImplementedException();
        }

        public Task Equip(int itemType, IEnumerable<(int, long)> itemList) {
            throw new System.NotImplementedException();
        }

        public Task<IBonusRewardPvp> GetBonusRewardPvp(string matchId, string adsId) {
            throw new System.NotImplementedException();
        }

        private class CoinRankingResult : ICoinRankingResult {
            public int RemainTime => 0;
            public ICoinRankingItemResult[] RankList => Array.Empty<ICoinRankingItemResult>();
            public ICoinRankingItemResult CurrentRank => new CoinRankingItemResult();

            private class CoinRankingItemResult : ICoinRankingItemResult {
                public int RankNumber {
                    get => 0;
                    set =>
                        RankNumber = value;
                }

                public string Name => "Mock";
                public float Point => 0;
            }
        }
    }
}