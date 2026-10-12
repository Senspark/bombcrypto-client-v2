using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts
{
    // Own type so the rent warning can map to a different prefab than the buy warning in the same loader
    public class DialogWarningBeforeRentHouse : DialogWarningBeforeBuyHouse {
        public new static UniTask<DialogWarningBeforeRentHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogWarningBeforeRentHouse>();
        }

        public static bool CanShowForRent(int slotLandRent) {
            return ServiceLocator.Instance.Resolve<IHouseStorageManager>().GetHouseSlot() + slotLandRent > 15;
        }
    }
}
