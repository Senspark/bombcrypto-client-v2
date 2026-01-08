using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimOfflineRewards : CmdSol {
        public CmdClaimOfflineRewards(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_OFFLINE_REWARDS_V2;
    }
}