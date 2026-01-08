using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdLeavePvpQueue : CmdSol {
        public CmdLeavePvpQueue(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.LEAVE_PVP_QUEUE_V2;
    }
}