using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdHeroTakeDamage : CmdSol {
        public CmdHeroTakeDamage(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.HERO_TAKE_DAMAGE_V2;
    }
}