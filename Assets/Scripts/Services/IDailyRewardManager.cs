using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IDailyRewardManager))]
    public interface IDailyRewardManager : IService, IEnumerable<DailyRewardData> {
        Task<IEnumerable<(int, string, int)>> ClaimRewardAsync();
        Task UpdateDataAsync();
    }
}