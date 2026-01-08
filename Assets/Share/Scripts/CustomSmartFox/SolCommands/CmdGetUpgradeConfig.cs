using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetUpgradeConfig : CmdSol {
        public CmdGetUpgradeConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_UPGRADE_CONFIG_V2;
    }
}