using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdKeepJoiningPvpQueue : CmdSol {
        public CmdKeepJoiningPvpQueue(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.KEEP_JOINING_PVP_QUEUE_V2;
    }
}