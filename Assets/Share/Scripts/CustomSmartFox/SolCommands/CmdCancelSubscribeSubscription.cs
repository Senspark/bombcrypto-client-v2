using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCancelSubscribeSubscription : CmdSol {
        public CmdCancelSubscribeSubscription(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CANCEL_SUBSCRIBE_SUBSCRIPTION;
    }
}