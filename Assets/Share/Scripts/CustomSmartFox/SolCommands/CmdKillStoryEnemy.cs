using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdKillStoryEnemy : CmdSol {
        public CmdKillStoryEnemy(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.KILL_STORY_ENEMY;
    }
}