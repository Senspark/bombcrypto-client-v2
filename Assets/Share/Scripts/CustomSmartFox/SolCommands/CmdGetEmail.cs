using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetEmail : CmdSol {
        public CmdGetEmail(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_EMAIL_V2;
    }
}