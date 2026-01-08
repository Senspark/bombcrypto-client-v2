using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetReward : CmdSol {
        public CmdGetReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_REWARD_V2;
    }
}