using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetAllClub : CmdSol {
        public CmdGetAllClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ALL_CLUB_V2;
    }
}