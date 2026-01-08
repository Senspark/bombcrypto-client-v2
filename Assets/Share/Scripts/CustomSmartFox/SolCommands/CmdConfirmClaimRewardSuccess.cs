using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdConfirmClaimRewardSuccess : CmdSol {
        public CmdConfirmClaimRewardSuccess(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CONFIRM_CLAIM_REWARD_SUCCESS_V2;
    }
}