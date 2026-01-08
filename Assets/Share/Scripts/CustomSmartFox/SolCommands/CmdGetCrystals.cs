using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetCrystals : CmdSol {
        public CmdGetCrystals(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_CRYSTALS_V2;
    }
}