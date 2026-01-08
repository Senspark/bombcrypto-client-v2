using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogLeaderboardPad : DialogLeaderboard {
        public new static UniTask<DialogLeaderboardPad> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLeaderboardPad>();
        }
    }
}
