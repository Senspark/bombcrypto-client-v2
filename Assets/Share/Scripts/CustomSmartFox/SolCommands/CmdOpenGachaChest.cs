using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdOpenGachaChest : CmdSol {
        public CmdOpenGachaChest(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.OPEN_GACHA_CHEST_V2;
    }
}