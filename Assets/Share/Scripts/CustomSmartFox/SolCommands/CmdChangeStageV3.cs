using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdChangeStageV3 : CmdSol {
        public CmdChangeStageV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CHANGE_BBM_STAGE_V3;
    }
}