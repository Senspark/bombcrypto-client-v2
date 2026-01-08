using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetCoinRanking : CmdSol {
        public CmdGetCoinRanking(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_COIN_RANKING_V2;
    }
}