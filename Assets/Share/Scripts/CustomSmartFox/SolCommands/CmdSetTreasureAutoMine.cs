using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSetTreasureAutoMine : CmdSol {
        public CmdSetTreasureAutoMine(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SET_TREASURE_AUTO_MINE;
    }
}
