using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUserLink : CmdSol {
        public CmdUserLink(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.USER_LINK_V2;
    }
}