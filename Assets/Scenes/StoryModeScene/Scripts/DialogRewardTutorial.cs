using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.StoryModeScene.Scripts {
    public class DialogRewardTutorial : BLDialogLuckyWheelReward {
        
        public new static async UniTask<ILuckyWheelReward> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogRewardTutorial>();
        }
    }
}