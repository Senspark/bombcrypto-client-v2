using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserWithdrawStake : CmdSol {
        public CmdUserWithdrawStake(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_WITHDRAW_STAKE_V2;
    }
}