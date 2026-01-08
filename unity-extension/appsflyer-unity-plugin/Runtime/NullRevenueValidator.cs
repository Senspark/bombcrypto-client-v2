using System;

using Manager;

namespace Senspark.Internal {
    internal class NullRevenueValidator : IRevenueValidator {
        public void Dispose() {
        }

        public int AddObserver(RevenueValidatorObserver observer) {
            return -1;
        }

        public bool RemoveObserver(int id) {
            return true;
        }

        public void DispatchEvent(Action<RevenueValidatorObserver> dispatcher) {
        }

        public PurchaseValidatedData LastPurchaseValidatedData { get; set; }
        public AdCampaignData LastAdCampaignData { get; set; }
    }
}