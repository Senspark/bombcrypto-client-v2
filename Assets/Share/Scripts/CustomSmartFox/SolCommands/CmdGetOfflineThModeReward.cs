using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetOfflineThModeReward : CmdSol {
        public CmdGetOfflineThModeReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_OFFLINE_TH_MODE_REWARD_V2;
    }
}