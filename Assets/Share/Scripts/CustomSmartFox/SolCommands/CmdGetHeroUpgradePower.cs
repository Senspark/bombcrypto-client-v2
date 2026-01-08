using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetHeroUpgradePower : CmdSol {
        public CmdGetHeroUpgradePower(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_HERO_UPGRADE_POWER_V2;
    }
}