using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCheckBomberStake : CmdSol {
        public CmdCheckBomberStake(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CHECK_BOMBER_STAKE_V2;
    }
}