using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartPve : CmdSol {
        public CmdStartPve(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_PVE_V2;
    }
}