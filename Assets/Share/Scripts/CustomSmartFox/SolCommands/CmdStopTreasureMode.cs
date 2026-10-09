using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStopTreasureMode : CmdSol {
        public CmdStopTreasureMode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.STOP_TREASURE_MODE;
    }
}
