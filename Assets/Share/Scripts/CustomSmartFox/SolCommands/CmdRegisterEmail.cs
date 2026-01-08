using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdRegisterEmail : CmdSol {
        public CmdRegisterEmail(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.REGISTER_EMAIL_V2;
    }
}