using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdAddHero : CmdSol {
        public CmdAddHero(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ADD_HERO_FOR_AIRDROP_USER;
    }
}