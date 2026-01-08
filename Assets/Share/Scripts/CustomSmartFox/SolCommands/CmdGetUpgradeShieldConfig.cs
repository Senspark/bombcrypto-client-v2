using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetUpgradeShieldConfig : CmdSol {
        public CmdGetUpgradeShieldConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_UPGRADE_SHIELD_CONFIG_V2;
    }
}