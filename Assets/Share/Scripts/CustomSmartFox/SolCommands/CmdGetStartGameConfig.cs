using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetStartGameConfig : CmdSol {
        public CmdGetStartGameConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_START_GAME_CONFIG_V2;
    }
}