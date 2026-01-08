using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetFreeGolds : CmdSol {
        public CmdGetFreeGolds(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_FREE_GOLDS_V2;
    }
}