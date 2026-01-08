using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncBomberMan : CmdSol {
        public CmdSyncBomberMan(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_BOMBERMAN_V3;
    }
}