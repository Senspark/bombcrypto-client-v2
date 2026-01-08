using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncDeposited : CmdSol {
        public CmdSyncDeposited(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_DEPOSITED_V3;
    }
}