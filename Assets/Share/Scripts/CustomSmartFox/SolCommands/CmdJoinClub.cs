using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdJoinClub : CmdSol {
        public CmdJoinClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.JOIN_CLUB_V2;
    }
}