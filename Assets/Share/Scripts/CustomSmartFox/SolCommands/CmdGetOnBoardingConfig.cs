using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetOnBoardingConfig : CmdSol {
        public CmdGetOnBoardingConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ON_BOARDING_CONFIG;
    }
}