using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdSellItemMarketV3 : CmdSol {
        public CmdSellItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SELL_ITEM_MARKET_V3;
    }
}