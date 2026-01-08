namespace Senspark.Iap.Receipts {
    public static class ReceiptParser {
        public static IReceipt Parse(string receipt) {
#if UNITY_EDITOR
            return new NullReceipt();
#elif UNITY_ANDROID
            return GoogleReceiptParser.Parse(receipt);
#else
            return new NullReceipt();
#endif
        }

        private class NullReceipt : IReceipt {
            public PurchaseState PurchaseState => PurchaseState.Purchased;
            public ConsumptionState ConsumptionState => ConsumptionState.Consumed;
            public string TransactionId => string.Empty;
            public bool IsTestPurchase => true;
        }
    }
}