using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSellItemMarketplace : CmdSol {
        public CmdSellItemMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SELL_ITEM_MARKETPLACE_V2;
    }
}