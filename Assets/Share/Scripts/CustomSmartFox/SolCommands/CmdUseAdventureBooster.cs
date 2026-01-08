using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUseAdventureBooster : CmdSol {
        public CmdUseAdventureBooster(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USE_ADVENTURE_BOOSTER_V2;
    }
}