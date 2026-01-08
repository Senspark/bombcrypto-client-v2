using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdAutoMinePrice : CmdSol {
        public CmdAutoMinePrice(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.AUTO_MINE_PRICE_V2;
    }
}