using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetRockPackConfigV3 : CmdSol {
        public CmdGetRockPackConfigV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ROCK_PACK_CONFIG_V3;
    }
}
