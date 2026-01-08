using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdWatchingDailyMissionAds : CmdSol {
        public CmdWatchingDailyMissionAds(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.WATCHING_DAILY_MISSION_ADS_V2;
    }
}