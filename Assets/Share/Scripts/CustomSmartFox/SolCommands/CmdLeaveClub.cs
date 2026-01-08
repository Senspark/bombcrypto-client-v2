using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdLeaveClub : CmdSol {
        public CmdLeaveClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.LEAVE_CLUB_V2;
    }
}