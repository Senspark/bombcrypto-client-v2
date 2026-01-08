using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDepositBasResponse : CmdSol {
        public CmdDepositBasResponse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DEPOSIT_BAS_RESPONSE;
    }
}