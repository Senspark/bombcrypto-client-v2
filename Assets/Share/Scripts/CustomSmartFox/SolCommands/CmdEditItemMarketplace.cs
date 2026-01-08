using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdEditItemMarketplace : CmdSol {
        public CmdEditItemMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.EDIT_ITEM_MARKETPLACE_V2;
    }
}