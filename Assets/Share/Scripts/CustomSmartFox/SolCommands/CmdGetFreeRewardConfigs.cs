using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetFreeRewardConfigs : CmdSol {
        public CmdGetFreeRewardConfigs(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_FREE_REWARD_CONFIGS_V2;
    }
}