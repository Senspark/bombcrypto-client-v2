using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetFreeHeroTraditional : CmdSol {
        public CmdGetFreeHeroTraditional(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_FREE_HERO_TRADITIONAL;
    }
}