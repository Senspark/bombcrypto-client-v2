using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdJoinAnotherClub : CmdSol {
        public CmdJoinAnotherClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.JOIN_CLUB_V3;
    }
}