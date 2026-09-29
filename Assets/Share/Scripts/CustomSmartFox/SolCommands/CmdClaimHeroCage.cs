using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdClaimHeroCage : CmdSol {
        public CmdClaimHeroCage(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CLAIM_HERO_CAGE;
    }
}
