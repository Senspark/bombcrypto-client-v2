using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetGemShopV3 : CmdSol {
        public CmdGetGemShopV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_GEM_SHOP_V3;
    }
}
