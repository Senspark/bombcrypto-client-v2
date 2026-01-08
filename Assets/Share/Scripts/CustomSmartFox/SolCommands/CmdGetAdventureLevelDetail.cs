using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetAdventureLevelDetail : CmdSol {
        public CmdGetAdventureLevelDetail(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ADVENTURE_LEVEL_DETAIL_V2;
    }
}