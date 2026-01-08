using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IBLTutorialManager))]
    public interface IBLTutorialManager : IService {
        void IncreaseTimePlayPvp();
        int TimePlayPvp();
    }
}