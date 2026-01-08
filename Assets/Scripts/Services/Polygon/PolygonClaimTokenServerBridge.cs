using Sfs2X.Entities.Data;

namespace App {
    public class 
        PolygonApproveClaimResponse : IApproveClaimResponse {
        public double Amount { get; }
        public string Signature { get; }
        public int TokenType { get; }
        public int Nonce { get; }
        public float ClaimedValue { get; }
        public string[] Details { get; }
        
        public PolygonApproveClaimResponse(ISFSObject data) {
            Amount = data.GetDouble("amount");
            Signature = data.GetUtfString("signature");
            TokenType = data.GetInt("tokenType");
            Nonce = data.GetInt("nonce");
            Details = data.GetUtfStringArray("details");
            ClaimedValue = 0;
        }
    }
    
    public class PolygonClaimTokenServerBridge : IClaimTokenServerBridge {
        public IApproveClaimResponse OnApproveClaim(ISFSObject data) {
            return new PolygonApproveClaimResponse(data);
        }
    }
}