using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetHeroesTraditional : CmdSol {
        public CmdGetHeroesTraditional(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_HEROES_TRADITIONAL_V3;
    }
}