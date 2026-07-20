using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCrosschainDepositBridgeNotify : CmdSol {
        public CmdCrosschainDepositBridgeNotify(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CROSSCHAIN_DEPOSIT_BRIDGE_NOTIFY;
    }
}
