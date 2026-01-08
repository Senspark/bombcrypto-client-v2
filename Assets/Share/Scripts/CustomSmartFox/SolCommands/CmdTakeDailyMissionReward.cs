using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdTakeDailyMissionReward : CmdSol {
        public CmdTakeDailyMissionReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.TAKE_DAILY_MISSION_REWARD_V2;
    }
}