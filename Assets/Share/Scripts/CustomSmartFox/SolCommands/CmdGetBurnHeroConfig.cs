using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetBurnHeroConfig : CmdSol {
        public CmdGetBurnHeroConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_BURN_HERO_CONFIG_V2;
    }
}