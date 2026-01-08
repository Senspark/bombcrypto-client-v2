using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdApproveClaim : CmdSol {
        public CmdApproveClaim(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.APPROVE_CLAIM_V2;
    }
}