using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetHeroUpgradeStamina : CmdSol {
        public CmdGetHeroUpgradeStamina(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_HERO_UPGRADE_STAMINA_V2;
    }
}
