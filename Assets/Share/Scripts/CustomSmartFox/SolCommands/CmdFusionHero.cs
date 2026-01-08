using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdFusionHero : CmdSol {
        public CmdFusionHero(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.FUSION_HERO_SERVER;
    }
}