using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimTaskReward : CmdSol {
        public CmdClaimTaskReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_TASK_REWARD_V2;
    }
}