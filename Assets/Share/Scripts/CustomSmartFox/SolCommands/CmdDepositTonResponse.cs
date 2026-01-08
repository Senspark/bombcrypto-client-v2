using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDepositTonResponse : CmdSol {
        public CmdDepositTonResponse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DEPOSIT_TON_RESPONSE;
    }
}