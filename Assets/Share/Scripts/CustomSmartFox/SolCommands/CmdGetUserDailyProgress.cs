using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetUserDailyProgress : CmdSol {
        public CmdGetUserDailyProgress(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_USER_DAILY_PROGRESS;
    }
}