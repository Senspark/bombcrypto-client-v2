using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGoWork : CmdSol {
        public CmdGoWork(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GO_WORK_V2;
    }
}