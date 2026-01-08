using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdActiveSkinChest : CmdSol {
        public CmdActiveSkinChest(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ACTIVE_SKIN_CHEST_V3;
    }
}