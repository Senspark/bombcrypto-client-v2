using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetBidPrice : CmdSol {
        public CmdGetBidPrice(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_BID_PRICE_V2;
    }
}