using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCancelSellItemMarketplace : CmdSol {
        public CmdCancelSellItemMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CANCEL_SELL_ITEM_MARKETPLACE_V2;
    }
}