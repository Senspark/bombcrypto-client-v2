using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdEnemyTakeDamage : CmdSol {
        public CmdEnemyTakeDamage(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ENEMY_TAKE_DAMAGE_V2;
    }
}