using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimReferralReward : CmdSol {
        public CmdClaimReferralReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_REFERRAL_REWARD_V2;
    }
}