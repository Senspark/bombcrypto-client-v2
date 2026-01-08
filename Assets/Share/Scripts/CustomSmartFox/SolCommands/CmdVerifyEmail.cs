using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdVerifyEmail : CmdSol {
        public CmdVerifyEmail(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.VERIFY_EMAIL_V2;
    }
}