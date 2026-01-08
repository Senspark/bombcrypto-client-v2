using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCompleteTask : CmdSol {
        public CmdCompleteTask(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.COMPLETE_TASK_V2;
    }
}