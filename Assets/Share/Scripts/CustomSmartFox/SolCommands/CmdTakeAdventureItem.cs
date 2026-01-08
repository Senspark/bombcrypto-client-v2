using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdTakeAdventureItem : CmdSol {
        public CmdTakeAdventureItem(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.TAKE_ADVENTURE_ITEM_V2;
    }
}