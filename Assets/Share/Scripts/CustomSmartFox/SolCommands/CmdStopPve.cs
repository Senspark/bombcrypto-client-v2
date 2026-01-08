using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStopPve : CmdSol {
        public CmdStopPve(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.STOP_PVE_V2;
    }
}