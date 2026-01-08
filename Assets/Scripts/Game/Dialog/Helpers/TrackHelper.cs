using Analytics;

using Services.IapAds;

namespace Utils {
    public static class TrackHelper {
        public static void TrackBuyIap(IAnalytics analytics, string productId, PurchaseResult result, int gemReceived) {
            productId ??= $"buy_{result.ProductId}";
            if (result.State == PurchaseState.Done) {
                analytics.TrackConversion(ConversionType.BuyIap);
                analytics.Iap_TrackBuyIap(result.TransactionId, productId, gemReceived, TrackResult.Done);
                var dataIap = new TrackDataIap(result.ProductId, IapConfig.ProductsPricesUsd[result.ProductId]);
                analytics.TrackData(dataIap.Name, dataIap.Parameters);
            } else {
                var trackResult = result.State switch {
                    PurchaseState.Cancel => TrackResult.Cancel,
                    _ => TrackResult.Error
                };
                analytics.TrackConversion(ConversionType.BuyIapFail);
                analytics.Iap_TrackBuyIap(result.TransactionId, productId, gemReceived, trackResult);
            }
        }
    }
}