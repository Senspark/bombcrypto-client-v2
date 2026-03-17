using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncMoreBomberMan : CmdSol {
        public CmdSyncMoreBomberMan(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_MORE_BOMBERMAN;
    }
}
