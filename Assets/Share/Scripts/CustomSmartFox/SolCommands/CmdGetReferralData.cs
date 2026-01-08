using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetReferralData : CmdSol {
        public CmdGetReferralData(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_REFERRAL_DATA_V2;
    }
}