using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdCancelItemMarketV3 : CmdSol {
        public CmdCancelItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CANCEL_SELL_ITEM_MARKET_V3;
    }
}