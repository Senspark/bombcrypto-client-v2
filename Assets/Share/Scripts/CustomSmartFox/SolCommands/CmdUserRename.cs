using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserRename : CmdSol {
        public CmdUserRename(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_RENAME_V2;
    }
}