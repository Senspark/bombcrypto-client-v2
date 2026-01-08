using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserBuyPvpBooster : CmdSol {
        public CmdUserBuyPvpBooster(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_BUY_PVP_BOOSTER;
    }
}