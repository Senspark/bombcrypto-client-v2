using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetOfflineRewards : CmdSol {
        public CmdGetOfflineRewards(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_OFFLINE_REWARDS_V2;
    }
}