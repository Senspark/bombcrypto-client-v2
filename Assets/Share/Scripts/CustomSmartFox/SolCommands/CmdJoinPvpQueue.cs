using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdJoinPvpQueue : CmdSol {
        public CmdJoinPvpQueue(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.JOIN_PVP_QUEUE_V2;
    }
}