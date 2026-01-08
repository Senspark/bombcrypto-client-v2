using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdDepositVicResponse : CmdSol {
        public CmdDepositVicResponse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.DEPOSIT_VIC_RESPONSE;
    }
}