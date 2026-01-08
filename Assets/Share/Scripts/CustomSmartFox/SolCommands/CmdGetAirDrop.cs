using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetAirDrop : CmdSol {
        public CmdGetAirDrop(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_AIR_DROP;
    }
}