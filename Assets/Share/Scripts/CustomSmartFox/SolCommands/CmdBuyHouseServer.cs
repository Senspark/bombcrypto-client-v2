using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdBuyHouseServer : CmdSol {
        public CmdBuyHouseServer(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.BUY_HOUSE_SERVER;
    }
}