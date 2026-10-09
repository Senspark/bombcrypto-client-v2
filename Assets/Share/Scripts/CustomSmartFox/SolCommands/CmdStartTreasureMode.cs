using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartTreasureMode : CmdSol {
        public CmdStartTreasureMode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_TREASURE_MODE;
    }
}
