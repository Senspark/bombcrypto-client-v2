using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands.Market {
    public class CmdGetMarketConfigV3 : CmdSol {
        public CmdGetMarketConfigV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_MARKET_CONFIG_V3;
    }
}