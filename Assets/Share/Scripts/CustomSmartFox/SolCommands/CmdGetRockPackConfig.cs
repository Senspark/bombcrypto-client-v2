using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetRockPackConfig : CmdSol {
        public CmdGetRockPackConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ROCK_PACK_CONFIG_V2;
    }
}