using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdRepairShieldBatch : CmdSol {
        public CmdRepairShieldBatch(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.REPAIR_SHIELD_BATCH;
    }
}
