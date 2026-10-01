using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdResumeTreasureMode : CmdSol {
        public CmdResumeTreasureMode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.RESUME_TREASURE_MODE;
    }
}
