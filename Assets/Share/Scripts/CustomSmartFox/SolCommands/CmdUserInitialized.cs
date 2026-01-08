using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserInitialized : CmdSol {
        public CmdUserInitialized(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_INITIALIZED;
    }
}