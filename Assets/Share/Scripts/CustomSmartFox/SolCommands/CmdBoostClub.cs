using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBoostClub : CmdSol {
        public CmdBoostClub(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BOOST_CLUB_V2;
    }
}