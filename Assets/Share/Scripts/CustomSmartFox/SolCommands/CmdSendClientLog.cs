using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSendClientLog : CmdSol {
        public CmdSendClientLog(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SEND_CLIENT_LOG;
    }
}