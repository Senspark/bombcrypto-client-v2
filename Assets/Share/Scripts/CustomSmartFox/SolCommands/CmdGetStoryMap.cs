using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetStoryMap : CmdSol {
        public CmdGetStoryMap(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_STORY_MAP;
    }
}