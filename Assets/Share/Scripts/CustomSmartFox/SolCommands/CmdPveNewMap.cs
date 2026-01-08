using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdPveNewMap : CmdSol {
        public CmdPveNewMap(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.PVE_NEW_MAP;
    }
}