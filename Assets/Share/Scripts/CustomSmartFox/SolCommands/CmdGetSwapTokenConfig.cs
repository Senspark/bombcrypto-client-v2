using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetSwapTokenConfig : CmdSol {
        public CmdGetSwapTokenConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_SWAP_TOKEN_CONFIG_V2;
    }
}