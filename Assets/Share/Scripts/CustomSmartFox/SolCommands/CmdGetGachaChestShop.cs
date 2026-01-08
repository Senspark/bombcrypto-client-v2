using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetGachaChestShop : CmdSol {
        public CmdGetGachaChestShop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_GACHA_CHEST_SHOP_V2;
    }
}