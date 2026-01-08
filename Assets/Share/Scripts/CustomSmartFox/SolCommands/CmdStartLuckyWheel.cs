using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdStartLuckyWheel : CmdSol {
        public CmdStartLuckyWheel(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.START_LUCKY_WHEEL;
    }
}