using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogInventoryPad : DialogInventory {
        public new static async UniTask<IDialogInventory> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInventoryPad>();
        }

        public new static async UniTask<IDialogInventory> CreateForFusion() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInventoryPad>();
        }
    }
}