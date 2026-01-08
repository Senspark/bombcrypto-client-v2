using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDeleteUser : CmdSol {
        public CmdDeleteUser(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DELETE_USER_V2;
    }
}