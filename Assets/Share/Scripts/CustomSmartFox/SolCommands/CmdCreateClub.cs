using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCreateClub : CmdSol {
        public CmdCreateClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CREATE_CLUB_V3;
    }
}