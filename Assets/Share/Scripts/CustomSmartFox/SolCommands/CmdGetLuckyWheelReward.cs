using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetLuckyWheelReward : CmdSol {
        public CmdGetLuckyWheelReward(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_LUCKY_WHEEL_REWARD_V2;
    }
}