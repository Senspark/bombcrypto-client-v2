using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyGachaChest : CmdSol {
        public CmdBuyGachaChest(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_GACHA_CHEST_V2;
    }
}