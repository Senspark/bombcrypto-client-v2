using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdPreviewToken : CmdSol {
        public CmdPreviewToken(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.PREVIEW_TOKEN_V2;
    }
}