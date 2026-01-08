using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    
    public class DialogHouseListPad : DialogHouseList {
        public new static async UniTask<IDialogHouseList> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHouseListPad>();
        }
    }
}