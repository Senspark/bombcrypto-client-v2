using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetGoldShop : CmdSol {
        public CmdGetGoldShop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_GOLD_SHOP_V2;
    }
}