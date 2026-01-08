using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUpgradeCrystal : CmdSol {
        public CmdUpgradeCrystal(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.UPGRADE_CRYSTAL_V2;
    }
}