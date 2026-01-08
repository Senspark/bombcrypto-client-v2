using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdGetMyItemMarketV3 : CmdSol {
        public CmdGetMyItemMarketV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_MY_ITEM_MARKET_V3;
    }
}