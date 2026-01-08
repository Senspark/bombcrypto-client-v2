using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdRepairShield : CmdSol {
        public CmdRepairShield(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.REPAIR_SHIELD_V2;
    }
}