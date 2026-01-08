using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetDailyTaskConfig : CmdSol {
        public CmdGetDailyTaskConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_DAILY_TASK_CONFIG;
    }
}