using System;

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public interface IUnityPurchaseManager : IDisposable {
        UniTask Initialize(float timeOut);
        string GetPrice(string productId);
        PriceValue GetPriceValue(string productId);

        [CanBeNull]
        SubscriptionInfo GetSubscriptionInfo(string productId);

        bool IsPurchased(string productId);
        UniTask<bool> Purchase(string productId);
        UniTask<bool> Restore();
        UniTask<bool> RemoveNonConsumable(string productId);
        UniTask<bool> RemoveAllNonConsumable();
    }
}