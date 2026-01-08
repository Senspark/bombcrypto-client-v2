using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetFreeGems : CmdSol {
        public CmdGetFreeGems(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_FREE_GEMS_V2;
    }
}