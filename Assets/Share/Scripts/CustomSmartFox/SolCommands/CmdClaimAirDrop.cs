using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimAirDrop : CmdSol {
        public CmdClaimAirDrop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_AIRDROP;
    }
}