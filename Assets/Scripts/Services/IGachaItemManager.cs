using Constant;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IGachaItemManager))]
    public interface IGachaItemManager {
        GachaItemData GetItem(GachaChestProductId productId);
    }
}