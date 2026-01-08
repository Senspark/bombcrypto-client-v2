using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetPackShop : CmdSol {
        public CmdGetPackShop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_PACK_SHOP_V2;
    }
}