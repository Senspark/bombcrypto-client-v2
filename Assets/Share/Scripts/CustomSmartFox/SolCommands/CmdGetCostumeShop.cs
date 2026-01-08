using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetCostumeShop : CmdSol {
        public CmdGetCostumeShop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_COSTUME_SHOP_V2;
    }
}