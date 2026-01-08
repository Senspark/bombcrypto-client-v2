using Senspark;

namespace Services {
    [Service(nameof(ISkinChestNameManager))]
    public interface ISkinChestNameManager {
        string GetName(int id);
    }
}