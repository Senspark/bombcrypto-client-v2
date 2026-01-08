using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdConfirmClaimAirDrop : CmdSol {
        public CmdConfirmClaimAirDrop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CONFIRM_CLAIM_AIRDROP;
    }
}