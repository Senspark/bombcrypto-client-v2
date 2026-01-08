using System.Threading.Tasks;

using App;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IPvPServerConfigManager))]
    public interface IPvPServerConfigManager : IService {
        ITaskDelay TaskDelay { get; }
        PvPServerConfigData GetConfig();
        Task InitializeAsync();
    }
}