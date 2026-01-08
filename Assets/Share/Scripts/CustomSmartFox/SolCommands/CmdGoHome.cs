using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGoHome : CmdSol {
        public CmdGoHome(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GO_HOME_V2;
    }
}