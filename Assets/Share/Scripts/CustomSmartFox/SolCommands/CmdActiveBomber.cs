using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdActiveBomber : CmdSol {
        public CmdActiveBomber(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ACTIVE_BOMBER_V2;
    }
}