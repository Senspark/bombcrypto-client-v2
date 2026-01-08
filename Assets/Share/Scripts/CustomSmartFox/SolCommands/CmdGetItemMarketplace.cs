using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetItemMarketplace : CmdSol {
        public CmdGetItemMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ITEM_MARKETPLACE_V2;
    }
}