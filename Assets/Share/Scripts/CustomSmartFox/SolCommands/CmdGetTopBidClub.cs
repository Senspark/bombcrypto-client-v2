using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetTopBidClub : CmdSol {
        public CmdGetTopBidClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_TOP_BID_CLUB_V2;
    }
}