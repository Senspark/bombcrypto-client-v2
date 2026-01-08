using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimDailyTask : CmdSol {
        public CmdClaimDailyTask(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_DAILY_TASK;
    }
}