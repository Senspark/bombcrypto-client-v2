using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetUserTasks : CmdSol {
        public CmdGetUserTasks(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_USER_TASKS_V2;
    }
}