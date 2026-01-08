using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdGetCurrentMinPriceMarketV3 : CmdSol {
        public CmdGetCurrentMinPriceMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_CURRENT_MARKET_MIN_PRICE_V3;
    }
}