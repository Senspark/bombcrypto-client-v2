using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUpgradeShieldLevel : CmdSol {
        public CmdUpgradeShieldLevel(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.UPGRADE_SHIELD_LEVEL_V2;
    }
}