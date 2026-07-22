using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdActiveBombers : CmdSol {
        public CmdActiveBombers(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ACTIVE_BOMBERS;
    }
}
