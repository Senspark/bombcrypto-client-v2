using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserStakeVip : CmdSol {
        public CmdUserStakeVip(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_STAKE_VIP;
    }
}