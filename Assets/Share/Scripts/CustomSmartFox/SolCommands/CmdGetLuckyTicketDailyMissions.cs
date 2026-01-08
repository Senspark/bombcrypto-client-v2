using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetLuckyTicketDailyMissions : CmdSol {
        public CmdGetLuckyTicketDailyMissions(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_LUCKY_TICKET_DAILY_MISSIONS;
    }
}