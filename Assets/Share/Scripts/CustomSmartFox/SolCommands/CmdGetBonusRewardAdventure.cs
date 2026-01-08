using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetBonusRewardAdventure : CmdSol {
        public CmdGetBonusRewardAdventure(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_BONUS_REWARD_ADVENTURE_V3;
    }
}