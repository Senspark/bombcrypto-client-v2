using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCheckNoAds : CmdSol {
        public CmdCheckNoAds(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CHECK_NO_ADS_V2;
    }
}