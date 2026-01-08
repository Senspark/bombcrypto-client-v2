using Data;

using Senspark;

namespace Services {
    [Service(nameof(IProductManager))]
    public interface IProductManager : IService {
        ProductData GetProduct(int productId);
        void Initialize(ProductData[] data);
    }
}