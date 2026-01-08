using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdCancelOrderItemMarketV3 : CmdSol {
        public CmdCancelOrderItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CANCEL_ORDER_ITEM_MARKET_V3;
    }
}