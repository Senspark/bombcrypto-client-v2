using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetGachaChests : CmdSol {
        public CmdGetGachaChests(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_GACHA_CHESTS_V2;
    }
}