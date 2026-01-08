using System.Collections.Generic;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IHeroColorManager))]
    public interface IHeroColorManager : IService {
        int GetColor(int heroId);
        void Initialize(IEnumerable<HeroColorData> data);
    }
}