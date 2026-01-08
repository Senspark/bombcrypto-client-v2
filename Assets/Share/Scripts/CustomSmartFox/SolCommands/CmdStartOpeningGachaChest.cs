using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartOpeningGachaChest : CmdSol {
        public CmdStartOpeningGachaChest(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_OPENING_GACHA_CHEST_V2;
    }
}