using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetDailyMission : CmdSol {
        public CmdGetDailyMission(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_DAILY_MISSION_V2;
    }
}