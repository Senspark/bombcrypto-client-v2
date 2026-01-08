using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetRankInfo : CmdSol {
        public CmdGetRankInfo(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_RANK_INFO_V2;
    }
}