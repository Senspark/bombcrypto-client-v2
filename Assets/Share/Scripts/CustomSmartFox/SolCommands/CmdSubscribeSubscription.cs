using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSubscribeSubscription : CmdSol {
        public CmdSubscribeSubscription(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SUBSCRIBE_SUBSCRIPTION;
    }
}