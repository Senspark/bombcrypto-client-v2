using System;

using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public interface IPurchaseController {
        bool IsInitialized();
        void Clear();
        UniTask<PurchaseResult> Purchase(string productId, ProductType productType);
        UniTask<bool> Restore();
        bool IsPurchased(string productId);
        bool Consume(string productId);
        Product FindProduct(string productId);
        Product[] FindProducts(string productId);
    }
}