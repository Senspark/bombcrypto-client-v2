using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimPvpMatchReward : CmdSol {
        public CmdClaimPvpMatchReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_PVP_MATCH_REWARD_V2;
    }
}