using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdChangeStage : CmdSol {
        public CmdChangeStage(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CHANGE_BBM_STAGE_V2;
    }
}