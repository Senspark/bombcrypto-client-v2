using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartAutoMine : CmdSol {
        public CmdStartAutoMine(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_AUTO_MINE_V2;
    }
}