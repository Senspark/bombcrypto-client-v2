using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyGachaChestSlot : CmdSol {
        public CmdBuyGachaChestSlot(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_GACHA_CHEST_SLOT_V2;
    }
}