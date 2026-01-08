using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetCoinLeaderboardConfig : CmdSol {
        public CmdGetCoinLeaderboardConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_COIN_LEADERBOARD_CONFIG_V2;
    }
}