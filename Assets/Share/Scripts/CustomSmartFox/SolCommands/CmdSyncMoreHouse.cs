using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncMoreHouse : CmdSol {
        public CmdSyncMoreHouse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_MORE_HOUSE;
    }
}
