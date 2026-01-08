using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGoSleep : CmdSol {
        public CmdGoSleep(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GO_SLEEP_V2;
    }
}