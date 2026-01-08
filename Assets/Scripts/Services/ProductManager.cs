using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    public class ProductManager : IProductManager {
        private IDictionary<int, ProductData> _data;
        private readonly ILogManager _logManager;

        public ProductManager(ILogManager logManager) {
            _logManager = logManager;
        }

        public void Destroy() {
        }

        public ProductData GetProduct(int productId) {
            return _data.TryGetValue(productId, out var value)
                ? value
                : throw new Exception($"Could not find product id: {productId}");
        }

        public void Initialize(ProductData[] data) {
            _data = data.ToDictionary(it => it.ItemId);
            _logManager.Log(string.Join(" | ", data.Select(it => $"item id: {it.ItemId}")));
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}