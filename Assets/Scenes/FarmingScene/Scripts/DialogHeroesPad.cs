
using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogHeroesPad : DialogHeroes {
        public new static async UniTask<IDialogHeroes> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHeroesPad>();
        }
    }
}