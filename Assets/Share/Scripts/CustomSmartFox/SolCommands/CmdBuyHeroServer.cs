using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyHeroServer : CmdSol {
        public CmdBuyHeroServer(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_HERO_SERVER;
    }
}