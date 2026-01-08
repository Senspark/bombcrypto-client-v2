using Sfs2X.Entities.Data;

namespace App {
    public interface IClaimTokenServerBridge {
        IApproveClaimResponse OnApproveClaim(ISFSObject data);
    }
}