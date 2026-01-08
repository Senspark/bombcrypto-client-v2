using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSwapToken : CmdSol {
        public CmdSwapToken(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SWAP_TOKEN_V2;
    }
}