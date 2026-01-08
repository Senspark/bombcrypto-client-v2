using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdOpenSkinChest : CmdSol {
        public CmdOpenSkinChest(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.OPEN_SKIN_CHEST_V2;
    }
}