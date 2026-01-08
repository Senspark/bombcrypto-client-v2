using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetHeroOldSeason : CmdSol {
        public CmdGetHeroOldSeason(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_HERO_OLD_SEASON;
    }
}