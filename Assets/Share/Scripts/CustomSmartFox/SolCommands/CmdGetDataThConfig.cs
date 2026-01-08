using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetDataThConfig : CmdSol {
        public CmdGetDataThConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_TH_DATA_CONFIG_V2;
    }
}