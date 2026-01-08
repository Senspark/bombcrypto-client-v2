using System.Collections.Generic;
using System.Threading.Tasks;

using Constant;

using Data;

using Services;

namespace Utils {
    public static class InventoryManagerExtension {
        public static Task<IEnumerable<InventoryItemData>> GetItemsAsync(this IInventoryManager inventoryManager,
            InventoryItemType itemType) {
            return inventoryManager.GetItemsAsync((int) itemType);
        }
    }
}