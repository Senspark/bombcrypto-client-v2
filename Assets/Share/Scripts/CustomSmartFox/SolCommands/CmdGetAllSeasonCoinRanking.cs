using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetAllSeasonCoinRanking : CmdSol {
        public CmdGetAllSeasonCoinRanking(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ALL_SEASON_COIN_RANKING_V2;
    }
}