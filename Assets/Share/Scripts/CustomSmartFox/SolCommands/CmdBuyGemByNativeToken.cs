using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyGemByNativeToken : CmdSol {
        public CmdBuyGemByNativeToken(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_GEM_BY_NATIVE_TOKEN;
    }
}
