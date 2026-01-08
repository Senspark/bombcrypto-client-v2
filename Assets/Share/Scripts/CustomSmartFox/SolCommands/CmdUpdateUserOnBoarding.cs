using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdUpdateUserOnBoarding : CmdSol {
        public CmdUpdateUserOnBoarding(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.UPDATE_USER_ON_BOARDING;
    }
}