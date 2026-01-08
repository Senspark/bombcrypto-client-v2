using Sfs2X.Entities.Data;

namespace App {
    public class BinanceClaimTokenServerBridge : IClaimTokenServerBridge {
        private class BinanceApproveClaimResponse : IApproveClaimResponse {
            public float ClaimedValue { get; }
            public BinanceApproveClaimResponse(ISFSObject data) {
                ClaimedValue = data.GetFloat("amount_of_bcoin_claimed");
            }
        }
        public IApproveClaimResponse OnApproveClaim(ISFSObject data) {
            return new BinanceApproveClaimResponse(data);
        }
    }
}