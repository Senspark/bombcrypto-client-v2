using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncHouse : CmdSol {
        public CmdSyncHouse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_HOUSE_V3;
    }
}