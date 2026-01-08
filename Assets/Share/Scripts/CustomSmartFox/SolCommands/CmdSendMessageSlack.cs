using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSendMessageSlack : CmdSol {
        public CmdSendMessageSlack(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SEND_MESSAGE_SLACK_V2;
    }
}