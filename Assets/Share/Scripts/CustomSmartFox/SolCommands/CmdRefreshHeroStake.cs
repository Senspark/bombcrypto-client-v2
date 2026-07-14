using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdRefreshHeroStake : CmdSol {
        public CmdRefreshHeroStake(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.REFRESH_HERO_STAKE;
    }
}
