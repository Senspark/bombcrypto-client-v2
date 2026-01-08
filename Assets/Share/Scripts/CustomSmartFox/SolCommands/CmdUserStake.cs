using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserStake : CmdSol {
        public CmdUserStake(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_STAKE_V2;
    }
}