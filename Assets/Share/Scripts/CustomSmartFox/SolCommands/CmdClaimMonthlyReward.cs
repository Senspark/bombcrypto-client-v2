using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimMonthlyReward : CmdSol {
        public CmdClaimMonthlyReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_MONTHLY_REWARD_V2;
    }
}