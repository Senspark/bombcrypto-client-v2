using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserClaimStakeVip : CmdSol {
        public CmdUserClaimStakeVip(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_CLAIM_STAKE_VIP;
    }
}