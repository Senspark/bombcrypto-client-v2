using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartExplode : CmdSol {
        public CmdStartExplode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_EXPLODE_V5;
    }
}