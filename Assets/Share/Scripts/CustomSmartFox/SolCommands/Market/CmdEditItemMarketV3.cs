using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdEditItemMarketV3 : CmdSol {
        public CmdEditItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.EDIT_ITEM_MARKET_V3;
    }
}