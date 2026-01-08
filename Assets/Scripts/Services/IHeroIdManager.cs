using System.Collections.Generic;

using Senspark;

namespace Services {
    [Service(nameof(IHeroIdManager))]
    public interface IHeroIdManager : IService {
        int GetHeroId(int itemId);
        int GetItemId(int heroId);
        void Initialize(IEnumerable<(int HeroId, int ItemId)> data);
    }
}