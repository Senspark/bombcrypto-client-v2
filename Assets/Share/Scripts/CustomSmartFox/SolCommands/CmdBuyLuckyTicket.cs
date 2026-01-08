using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyLuckyTicket : CmdSol {
        public CmdBuyLuckyTicket(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_LUCKY_TICKET;
    }
}