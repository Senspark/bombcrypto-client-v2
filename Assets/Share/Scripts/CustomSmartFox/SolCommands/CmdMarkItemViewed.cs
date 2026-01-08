using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdMarkItemViewed : CmdSol {
        public CmdMarkItemViewed(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.MARK_ITEM_VIEWED_V2;
    }
}