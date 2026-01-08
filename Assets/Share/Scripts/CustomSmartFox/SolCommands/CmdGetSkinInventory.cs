using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetSkinInventory : CmdSol {
        public CmdGetSkinInventory(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_SKIN_INVENTORY_V2;
    }
}