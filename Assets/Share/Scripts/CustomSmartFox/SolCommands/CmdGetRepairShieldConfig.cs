using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetRepairShieldConfig : CmdSol {
        public CmdGetRepairShieldConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_REPAIR_SHIELD_CONFIG_V2;
    }
}