using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdChangeMiningToken : CmdSol {
        public CmdChangeMiningToken(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CHANGE_MINING_TOKEN;
    }
}