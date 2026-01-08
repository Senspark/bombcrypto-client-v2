using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartStoryExplode : CmdSol {
        public CmdStartStoryExplode(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_STORY_EXPLODE_V2;
    }
}