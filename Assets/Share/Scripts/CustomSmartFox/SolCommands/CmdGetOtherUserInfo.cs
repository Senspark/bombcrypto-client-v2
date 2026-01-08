using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetOtherUserInfo : CmdSol {
        public CmdGetOtherUserInfo(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_OTHER_USER_INFO_V2;
    }
}