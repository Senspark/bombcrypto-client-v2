using System.Threading.Tasks;
using App;
using Cysharp.Threading.Tasks;
using Scenes.FarmingScene.Scripts;

namespace Game.Manager {
    public class DefaultFusionManager : IFusionManager {
        private readonly NetworkType _networkType = NetworkType.Polygon;

        public async UniTask<Dialog.Dialog> CreateDialog() {
            if (AppConfig.IsTon() || AppConfig.IsWebAirdrop()) {
                return await DialogFusionAirdrop.Create();
            }
            return await DialogFusionPolygon.Create();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}