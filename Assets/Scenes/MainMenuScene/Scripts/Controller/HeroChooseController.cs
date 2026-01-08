using System.Linq;
using System.Threading.Tasks;

using App;

using Senspark;

using Services;

namespace Scenes.MainMenuScene.Scripts.Controller {
    public class HeroChooseController {
        
        private IStorageManager _storeManager;
        private IEarlyConfigManager _configManager;
        private ITRHeroManager _trHeroManager;
        private IPvPBombRankManager _rankManager;
        private ISkinManager _skinManager;

        public async Task Initialized() {
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _configManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _rankManager = ServiceLocator.Instance.Resolve<IPvPBombRankManager>();
            _skinManager = ServiceLocator.Instance.Resolve<ISkinManager>();
            await _rankManager.InitializeAsync();
        }

        public async Task<int> GetHeroChoose() {
            var result = await _trHeroManager.GetHeroesAsync("HERO");
            var heroes = result.ToArray();
            if (heroes.Length == 0) {
                return -1;
            }
            foreach (var iter in heroes) {
                if (iter.IsActive) {
                    return iter.ItemId;
                }
            }
            return heroes[0].ItemId;
        }

        public async Task<ISkinManager.Skin[]> GetSkins() {
            var skins = await _skinManager.GetSkinsEquipped();
            return skins.ToArray();
        }

        public PvpRankType GetRankType() {
            return _rankManager.GetBombRank();
        }
        
        public int GetCurrentPoint() {
            return _rankManager.GetCurrentPoint();
        }
        
        public int GetDecayPointUser() {
            return _rankManager.GetDecayPointUser();
        }

        public (int startPoint, int endPoint) GetStartEndPoint() {
            var bombRank = (int) _rankManager.GetBombRank();
            var ranks = _configManager.Ranks;
            foreach (var iter in ranks) {
                var rank = iter.BombRank;
                if (rank == bombRank) {
                    return (iter.StartPoint, iter.EndPoint);
                }
            }
            return (0, 0);
        }
    }
}