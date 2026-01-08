using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdOrderItemMarketV3 : CmdSol {
        public CmdOrderItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ORDER_ITEM_MARKET_V3;
    }
}