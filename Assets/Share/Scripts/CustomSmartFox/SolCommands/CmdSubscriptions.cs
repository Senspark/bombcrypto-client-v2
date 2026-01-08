using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSubscriptions : CmdSol {
        public CmdSubscriptions(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SUBSCRIPTIONS;
    }
}