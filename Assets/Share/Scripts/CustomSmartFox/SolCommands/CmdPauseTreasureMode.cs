using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdPauseTreasureMode : CmdSol {
        public CmdPauseTreasureMode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.PAUSE_TREASURE_MODE;
    }
}
