using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyCostumeItem : CmdSol {
        public CmdBuyCostumeItem(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_COSTUME_ITEM_V2;
    }
}