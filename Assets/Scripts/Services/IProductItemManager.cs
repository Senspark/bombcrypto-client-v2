using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IProductItemManager))]
    public interface IProductItemManager : IService {
        Task<string> GetDescriptionAsync(int itemId);
        string GetDescription(int itemId);
        ProductItemData GetItem(int itemId);
        Task<ProductItemData> GetItemAsync(int itemId);
        Task InitializeAsync();
    }
}