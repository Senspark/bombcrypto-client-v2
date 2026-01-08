using System.Threading.Tasks;
using Senspark;

namespace Services {

    public enum PvpRankType {
        Unknown,
        Iron1,
        Iron2,
        Copper1,
        Copper2,
        Silver1,
        Silver2,
        Gold1,
        Gold2,
        Platinum1,
        Platinum2,
        Emerald1,
        Emerald2,
        Diamond1,
        Diamond2
    }
    
    public class PvPBombRankManager : IPvPBombRankManager {
        private IRankInfoManager _rankInfoManager;
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public PvpRankType GetBombRank() {
            return GetBombRank(_rankInfoManager.BombRank);
        }

        public PvpRankType GetBombRank(int bombRank) {
            return bombRank switch {
                1 => PvpRankType.Iron1,
                2 => PvpRankType.Iron2,
                3 => PvpRankType.Copper1,
                4 => PvpRankType.Copper2,
                5 => PvpRankType.Silver1,
                6 => PvpRankType.Silver2,
                7 => PvpRankType.Gold1,
                8 => PvpRankType.Gold2,
                9 => PvpRankType.Platinum1,
                10 => PvpRankType.Platinum2,
                11 => PvpRankType.Emerald1,
                12 => PvpRankType.Emerald2,
                13 => PvpRankType.Diamond1,
                14 => PvpRankType.Diamond2,
                _ => PvpRankType.Unknown
            };
        }

        public int GetCurrentPoint() {
            return _rankInfoManager.CurrentPoint;
        }
        
        public int GetDecayPointConfig() {
            return _rankInfoManager.DecayPointConfig;
        }
        
        public int GetMinMatchesConfig() {
            return _rankInfoManager.MinMatchesConfig;
        }
        
        public int GetAmountMatches() {
            return _rankInfoManager.AmountMatches;
        }
        
        public int GetDecayPointUser() {
            return _rankInfoManager.DecayPointUser;
        }

        public async Task InitializeAsync() {
            _rankInfoManager ??= ServiceLocator.Instance.Resolve<IRankInfoManager>();
            await _rankInfoManager.InitializeAsync();
        }
    }
}