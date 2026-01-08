using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdTest : CmdSol {
        private readonly string _cmd;
        public CmdTest(ISFSObject data, string cmd) : base(data) {
            _cmd = cmd;
        }

        public override string Cmd => _cmd;
    }
}