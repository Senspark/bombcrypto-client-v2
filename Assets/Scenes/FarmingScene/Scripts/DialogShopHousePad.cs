using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogShopHousePad : DialogShopHouse {
        public new static UniTask<DialogShopHousePad> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHousePad>();
        }
    }
}