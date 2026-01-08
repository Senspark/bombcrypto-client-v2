using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncPvpConfig : CmdSol {
        public CmdSyncPvpConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_PVP_CONFIG_V2;
    }
}