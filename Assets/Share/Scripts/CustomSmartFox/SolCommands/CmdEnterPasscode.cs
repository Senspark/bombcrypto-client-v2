using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdEnterPasscode : CmdSol {
        public CmdEnterPasscode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ENTER_PASSCODE;
    }
}