using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.FarmingScene.Scripts {
    public class DialogBridgeWithdrawAmount : DialogBridgeAmount {
        public new static UniTask<DialogBridgeWithdrawAmount> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                .Instantiate<DialogBridgeWithdrawAmount>();
        }
    }
}
