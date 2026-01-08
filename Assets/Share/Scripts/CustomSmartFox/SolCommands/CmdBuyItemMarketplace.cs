using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyItemMarketplace : CmdSol {
        public CmdBuyItemMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_ITEM_MARKETPLACE_V2;
    }
}