using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUpgradeShieldLevelV3 : CmdSol {
        public CmdUpgradeShieldLevelV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.UPGRADE_SHIELD_LEVEL_V3;
    }
}
