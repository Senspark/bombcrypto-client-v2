using Cysharp.Threading.Tasks;
using Senspark;

namespace Game.Manager {
    [Service(nameof(IFusionManager))]
    public interface IFusionManager : IService {
        UniTask<Dialog.Dialog> CreateDialog();
    }
}