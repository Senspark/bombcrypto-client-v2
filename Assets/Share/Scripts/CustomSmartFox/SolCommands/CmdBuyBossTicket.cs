using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyBossTicket : CmdSol {
        public CmdBuyBossTicket(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_BOSS_TICKET;
    }
}