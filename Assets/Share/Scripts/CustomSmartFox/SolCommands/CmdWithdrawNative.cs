using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdWithdrawNative : CmdSol {
        public CmdWithdrawNative(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.WITHDRAW_NATIVE;
    }
}
