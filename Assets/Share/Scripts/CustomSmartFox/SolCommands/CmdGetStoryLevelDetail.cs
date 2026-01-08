using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetStoryLevelDetail : CmdSol {
        public CmdGetStoryLevelDetail(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_STORY_LEVEL_DETAIL;
    }
}