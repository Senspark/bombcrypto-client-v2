using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.FarmingScene.Scripts;

using Senspark;

namespace App {
    [Service(nameof(IRepairShieldManager))]
    public interface IRepairShieldManager : IService {
        UniTask<IDialogRepairShield> CreateDialog();
    }
}