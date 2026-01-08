using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCreateRock : CmdSol {
        public CmdCreateRock(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CREATE_ROCK_V2;
    }
}