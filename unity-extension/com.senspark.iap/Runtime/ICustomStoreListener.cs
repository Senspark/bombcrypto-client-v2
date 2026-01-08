using System;
using Cysharp.Threading.Tasks;

namespace Senspark.Iap {
    public interface ICustomStoreListener {
        event Action<PurchaseRawReceipt> OnPurchaseHasResult;
        event Action<InitResult> OnInitializeSuccess;
        UniTask<bool> SetIapData(IapData[] iapData);
    }
}