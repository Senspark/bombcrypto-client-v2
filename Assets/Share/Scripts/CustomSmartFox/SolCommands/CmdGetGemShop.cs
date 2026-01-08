using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetGemShop : CmdSol {
        public CmdGetGemShop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_GEM_SHOP_V2;
    }
}