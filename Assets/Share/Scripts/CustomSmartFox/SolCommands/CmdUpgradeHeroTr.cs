using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUpgradeHeroTr : CmdSol {
        public CmdUpgradeHeroTr(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.UPGRADE_HERO_TR_V2;
    }
}