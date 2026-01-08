using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IShopManager))]
    public interface IShopManager : IService {
        Task BuyCostumeAsync(int itemId, string itemPackage, int quantity);
        CostumeData[] GetCostumes(int itemType);
        Task<CostumeData[]> GetCostumesAsync(int itemType);
        public void RemoveItemShop(int itemId, int itemType);
    }
}