using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdAdventureReviveHero : CmdSol {
        public CmdAdventureReviveHero(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ADVENTURE_REVIVE_HERO_V2;
    }
}