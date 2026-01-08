using Senspark;

namespace Services {
    [Service(nameof(IGachaChestNameManager))]
    public interface IGachaChestNameManager : IService {
        string GetChestName(int chestType);
    }
}