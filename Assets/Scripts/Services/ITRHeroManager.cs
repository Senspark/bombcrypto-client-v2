using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(ITRHeroManager))]
    public interface ITRHeroManager : IService {
        /// <summary>
        /// type:
        /// HERO (default)
        /// SOUL
        /// </summary>
        Task<IEnumerable<TRHeroData>> GetHeroesAsync(string type);
    }
}