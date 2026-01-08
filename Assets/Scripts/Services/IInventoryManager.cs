using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IInventoryManager))]
    public interface IInventoryManager : IService {
        void Clear();
        Task<IEnumerable<InventoryChestData>> GetChestAsync();
        Task<IEnumerable<InventoryHeroData>> GetHeroesAsync();
        Task<IEnumerable<InventoryItemData>> GetItemsAsync(int itemType);
        Task<IEnumerable<InventorySellingItemData>> GetSellingItemsAsync();
        Task UnlockChestSlotAsync(int slotId);
        Task<int> GetCurrentAvatarTR();
    }
}