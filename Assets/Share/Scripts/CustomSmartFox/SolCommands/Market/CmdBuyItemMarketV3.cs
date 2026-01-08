using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdBuyItemMarketV3 : CmdSol {
        public CmdBuyItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_ITEM_MARKET_V3;
    }
}