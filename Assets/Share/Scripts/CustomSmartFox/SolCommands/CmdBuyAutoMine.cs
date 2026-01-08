using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyAutoMine : CmdSol {
        public CmdBuyAutoMine(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_AUTO_MINE_V2;
    }
}