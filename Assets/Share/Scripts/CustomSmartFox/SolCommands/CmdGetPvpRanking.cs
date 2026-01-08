using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetPvpRanking : CmdSol {
        public CmdGetPvpRanking(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_PVP_RANKING_V2;
    }
}