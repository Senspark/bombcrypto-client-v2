using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdActiveHouse : CmdSol {
        public CmdActiveHouse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ACTIVE_HOUSE_V2;
    }
}