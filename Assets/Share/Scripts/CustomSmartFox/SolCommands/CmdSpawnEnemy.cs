using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSpawnEnemy : CmdSol {
        public CmdSpawnEnemy(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SPAWN_ENEMY_V2;
    }
}