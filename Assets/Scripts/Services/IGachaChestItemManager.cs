using Senspark;

using Game.Dialog.BomberLand.BLFrameShop;

namespace Services {
    [Service(nameof(IGachaChestItemManager))]
    public interface IGachaChestItemManager : IService {
        void SetItems(ChestShopType chestType, int[] items);
        int[] GetItems(ChestShopType chestType);
    }
}