using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetMinStakeHero : CmdSol {
        public CmdGetMinStakeHero(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_MIN_STAKE_HERO_V2;
    }
}