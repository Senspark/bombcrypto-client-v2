using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class InitResult {
        public readonly bool IsSuccess;
        public readonly IStoreController Controller;
        public readonly IExtensionProvider Extension;

        public InitResult() {
            IsSuccess = false;
        }

        public InitResult(IStoreController controller, IExtensionProvider extension) {
            Controller = controller;
            Extension = extension;
            IsSuccess = controller != null && extension != null;
        }
    }

    public readonly struct PurchaseResult {
        public readonly bool IsSuccess;
        public readonly PurchaseFrom PurchaseFrom;
        public readonly string OrderId;
        public readonly string Receipt;

        public static PurchaseResult Fail => new(false, string.Empty, string.Empty, PurchaseFrom.Unknown);

        public PurchaseResult(bool isSuccess, string orderId, string receipt, PurchaseFrom purchaseFrom) {
            IsSuccess = isSuccess;
            OrderId = orderId;
            Receipt = receipt;
            PurchaseFrom = purchaseFrom;
        }
    }
}