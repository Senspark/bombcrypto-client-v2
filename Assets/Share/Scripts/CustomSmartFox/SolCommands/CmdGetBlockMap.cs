using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetBlockMap : CmdSol {
        public CmdGetBlockMap(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_BLOCK_MAP_V2;
    }
}