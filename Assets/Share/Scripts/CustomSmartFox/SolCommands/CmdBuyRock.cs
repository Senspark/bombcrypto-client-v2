using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyRock : CmdSol {
        public CmdBuyRock(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_ROCK_V2;
    }
}