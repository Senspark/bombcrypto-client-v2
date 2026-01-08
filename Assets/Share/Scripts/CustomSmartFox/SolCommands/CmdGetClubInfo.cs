using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetClubInfo : CmdSol {
        public CmdGetClubInfo(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_CLUB_INFO_V2;
    }
}