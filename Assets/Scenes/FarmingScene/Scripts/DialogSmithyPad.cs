
using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogSmithyPad : DialogSmithyPolygon {
        public new static UniTask<DialogSmithyPad> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogSmithyPad>();
        }
        
    }
}