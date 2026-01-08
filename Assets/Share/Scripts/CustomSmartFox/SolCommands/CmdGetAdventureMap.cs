using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetAdventureMap : CmdSol {
        public CmdGetAdventureMap(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ADVENTURE_MAP_V2;
    }
}