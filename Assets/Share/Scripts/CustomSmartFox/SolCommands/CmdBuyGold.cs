using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyGold : CmdSol {
        public CmdBuyGold(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_GOLD_V2;
    }
}