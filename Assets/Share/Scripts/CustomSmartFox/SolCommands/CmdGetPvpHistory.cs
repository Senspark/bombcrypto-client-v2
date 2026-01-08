using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetPvpHistory : CmdSol {
        public CmdGetPvpHistory(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_PVP_HISTORY_V2;
    }
}