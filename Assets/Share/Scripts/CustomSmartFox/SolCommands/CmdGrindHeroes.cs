using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGrindHeroes : CmdSol {
        public CmdGrindHeroes(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GRIND_HEROES_V2;
    }
}