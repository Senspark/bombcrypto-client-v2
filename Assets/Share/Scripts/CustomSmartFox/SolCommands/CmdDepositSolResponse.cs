using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDepositSolResponse : CmdSol {
        public CmdDepositSolResponse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DEPOSIT_SOL_RESPONSE;
    }
}