using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogLeaderboardInformation : Dialog {
        public static UniTask<DialogLeaderboardInformation> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLeaderboardInformation>();
        }
    }
}