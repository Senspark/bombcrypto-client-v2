using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimLuckyTicketDailyMission : CmdSol {
        public CmdClaimLuckyTicketDailyMission(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_LUCKY_TICKET_DAILY_MISSION;
    }
}