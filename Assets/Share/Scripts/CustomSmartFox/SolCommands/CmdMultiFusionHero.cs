using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdMultiFusionHero : CmdSol {
        public CmdMultiFusionHero(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.MULTI_FUSION_HERO_SERVER;
    }
}