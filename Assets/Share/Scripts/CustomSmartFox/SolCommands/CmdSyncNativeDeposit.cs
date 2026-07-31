using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncNativeDeposit : CmdSol {
        public CmdSyncNativeDeposit(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_NATIVE_DEPOSIT;
    }
}
