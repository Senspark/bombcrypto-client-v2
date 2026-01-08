using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetPvpServerConfigs : CmdSol {
        public CmdGetPvpServerConfigs(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_PVP_SERVER_CONFIGS_V2;
    }
}