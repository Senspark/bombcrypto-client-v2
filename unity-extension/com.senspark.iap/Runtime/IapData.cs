using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class IapData {
        public readonly string ProductId;
        public readonly ProductType Type;
        public readonly PriceValue Price;
        
        public IapData(string productId, ProductType type, float price) {
            ProductId = productId;
            Type = type;
            Price = new PriceValue(price, "USD");
        }

        public IapData(string productId, ProductType type, PriceValue price) {
            ProductId = productId;
            Type = type;
            Price = price;
        }
    }
}