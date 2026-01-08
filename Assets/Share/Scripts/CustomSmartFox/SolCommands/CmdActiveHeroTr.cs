using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdActiveHeroTr : CmdSol {
        public CmdActiveHeroTr(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ACTIVE_HERO_TR_V2;
    }
}