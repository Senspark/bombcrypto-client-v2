using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdThModeRewards : CmdSol {
        public CmdThModeRewards(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.TH_MODE_V2_REWARDS;
    }
}