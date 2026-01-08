using System.Collections.Generic;

using Senspark;

namespace Analytics {
    public class TrackDataIap : IAnalyticsEvent {
        public string Name => $"conversion_buy_{_packageId}";
        public Dictionary<string, object> Parameters { get; }
        private readonly string _packageId;

        public TrackDataIap(string packageId, float priceUsd) {
            _packageId = packageId;
            Parameters = new Dictionary<string, object> {
                { "af_currency", "USD" },
                { "af_revenue", priceUsd },
                { "af_quantity", 1 },
                { "af_content_type", "iap_product" },
                { "af_content_id", packageId },
            };
        }
    }
}