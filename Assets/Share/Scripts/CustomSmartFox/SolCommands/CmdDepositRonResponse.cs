using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDepositRonResponse : CmdSol {
        public CmdDepositRonResponse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DEPOSIT_RON_RESPONSE;
    }
}