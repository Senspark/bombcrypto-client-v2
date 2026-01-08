using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetBonusRewardPvp : CmdSol {
        public CmdGetBonusRewardPvp(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_BONUS_REWARD_PVP_V3;
    }
}