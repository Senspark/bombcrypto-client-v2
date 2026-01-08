using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyPvpTicket : CmdSol {
        public CmdBuyPvpTicket(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_PVP_TICKET;
    }
}